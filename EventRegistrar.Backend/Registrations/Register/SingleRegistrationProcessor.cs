using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Spots;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class SingleRegistrationProcessor
    {
        private readonly IQueryable<QuestionOptionToRegistrableMapping> _optionToRegistrableMappings;
        private readonly PhoneNormalizer _phoneNormalizer;
        private readonly PriceCalculator _priceCalculator;
        private readonly IRepository<Registration> _registrations;
        private readonly SeatManager _seatManager;
        private readonly ServiceBusClient _serviceBusClient;

        public SingleRegistrationProcessor(PhoneNormalizer phoneNormalizer,
                                           IQueryable<QuestionOptionToRegistrableMapping> optionToRegistrableMappings,
                                           SeatManager seatManager,
                                           PriceCalculator priceCalculator,
                                           IRepository<Registration> registrations,
                                           ServiceBusClient serviceBusClient)
        {
            _phoneNormalizer = phoneNormalizer;
            _optionToRegistrableMappings = optionToRegistrableMappings;
            _seatManager = seatManager;
            _priceCalculator = priceCalculator;
            _registrations = registrations;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<IEnumerable<Seat>> Process(Registration registration, SingleRegistrationProcessConfiguration config)
        {
            registration.RespondentFirstName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_FirstName)?.ResponseString;
            registration.RespondentLastName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_LastName)?.ResponseString;
            registration.RespondentEmail = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Email)?.ResponseString;
            registration.IsReduced = config.QuestionOptionId_Reduction != null
                                  && registration.Responses.Any(rsp => rsp.QuestionId == config.QuestionOptionId_Reduction.Value);
            if (config.QuestionId_Phone != null)
            {
                registration.Phone = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Phone.Value)?.ResponseString;
                registration.PhoneNormalized = _phoneNormalizer.NormalizePhone(registration.Phone);
            }
            if (config.LanguageMappings != null)
            {
                registration.Language = config.LanguageMappings.FirstOrDefault(map => registration.Responses.Any(rsp => rsp.QuestionOptionId == map.QuestionOptionId)).Language;
                if (string.IsNullOrEmpty(registration.Language))
                {
                    registration.Language = null;
                }
            }
            if (config.QuestionId_Remarks != null)
            {
                registration.Remarks = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Remarks.Value)?.ResponseString;
                if (string.IsNullOrEmpty(registration.Remarks))
                {
                    registration.Remarks = null;
                }
            }
            var ownSeats = new List<Seat>();

            var questionOptionIds = new HashSet<Guid>(registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value));
            var mappings = await _optionToRegistrableMappings
                                 .Where(map => questionOptionIds.Contains(map.QuestionOptionId))
                                 .Include(map => map.Registrable)
                                 .Include(map => map.Registrable.Seats)
                                 .ToListAsync();

            var soldOutMessages = new StringBuilder();
            foreach (var response in registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue))
            {
                foreach (var mapping in mappings.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
                {
                    var isDoubleRegistrable = mapping.Registrable.MaximumDoubleSeats.HasValue;
                    Seat seat;
                    if (isDoubleRegistrable)
                    {
                        var partnerOriginal = mapping.QuestionId_Partner.HasValue
                                              ? registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == mapping.QuestionId_Partner)?.ResponseString
                                              : null;
                        if (string.IsNullOrWhiteSpace(partnerOriginal))
                        {
                            partnerOriginal = null;
                        }

                        var partnerNormalized = partnerOriginal?.ToLowerInvariant();

                        var questionOptionId_Leader = mapping.QuestionOptionId_Leader ?? config.QuestionOptionId_Leader;
                        var questionOptionId_Follower = mapping.QuestionOptionId_Follower ?? config.QuestionOptionId_Follower;
                        var isLeader = questionOptionId_Leader.HasValue && registration.Responses.Any(rsp => rsp.QuestionOptionId == questionOptionId_Leader.Value);
                        var isFollower = questionOptionId_Follower.HasValue && registration.Responses.Any(rsp => rsp.QuestionOptionId == questionOptionId_Follower.Value);
                        var role = isLeader ? Role.Leader : (isFollower ? Role.Follower : (Role?)null);
                        var ownIdentification = new RegistrationIdentification(registration);
                        seat = await _seatManager.ReserveSinglePartOfPartnerSpot(registration.EventId, mapping.Registrable, registration.Id, ownIdentification, partnerNormalized, role, true);

                        registration.PartnerNormalized = (partnerNormalized ?? registration.PartnerNormalized)?.ToLowerInvariant();
                        registration.PartnerOriginal = partnerOriginal ?? registration.PartnerOriginal;
                        if (seat.IsPartnerSpot && registration.RegistrationId_Partner == null)
                        {
                            registration.RegistrationId_Partner = seat.GetOtherRegistrationId(registration.Id);
                            // set own id as partner id of partner registration
                            var partnerRegistration = await _registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner);
                            if (partnerRegistration != null)
                            {
                                partnerRegistration.RegistrationId_Partner = registration.Id;
                            }
                        }
                    }
                    else
                    {
                        seat = await _seatManager.ReserveSingleSpot(registration.EventId, mapping.Registrable, registration.Id, true);
                    }
                    if (seat == null)
                    {
                        soldOutMessages.AppendLine((registration.SoldOutMessage == null ? string.Empty : registration.SoldOutMessage + Environment.NewLine) +
                                                    string.Format(Properties.Resources.RegistrableSoldOut, mapping.Registrable.Name));
                    }
                    else
                    {
                        ownSeats.Add(seat);
                    }
                }
            }

            registration.SoldOutMessage = soldOutMessages.ToString();
            if (string.IsNullOrEmpty(registration.SoldOutMessage))
            {
                registration.SoldOutMessage = null;
            }

            var isOnWaitingList = ownSeats.Any(seat => seat.IsWaitingList);
            registration.IsWaitingList = isOnWaitingList;
            if (registration.IsWaitingList == false && !registration.AdmittedAt.HasValue)
            {
                registration.AdmittedAt = DateTime.UtcNow;
            }

            registration.OriginalPrice = await _priceCalculator.CalculatePrice(registration, registration.Responses, ownSeats);
            registration.Price = registration.OriginalPrice;

            await _registrations.InsertOrUpdateEntity(registration);

            // send mail
            var isPartnerRegistration = registration.PartnerNormalized != null;
            var isUnmatchedPartnerRegistration = isPartnerRegistration
                                              && ownSeats.Any(seat => seat.PartnerEmail != null
                                                                   && (!seat.RegistrationId.HasValue || !seat.RegistrationId_Follower.HasValue));
            MailType mailToSend;
            var withhold = false;
            if (registration.OriginalPrice == 0m && !string.IsNullOrEmpty(registration.SoldOutMessage))
            {
                mailToSend = MailType.SoldOut;
                withhold = true;
            }
            else
            {
                if (!isPartnerRegistration)
                {
                    mailToSend = isOnWaitingList
                        ? MailType.SingleRegistrationOnWaitingList
                        : MailType.SingleRegistrationAccepted;
                }
                else if (isUnmatchedPartnerRegistration)
                {
                    mailToSend = isOnWaitingList
                        ? MailType.PartnerRegistrationFirstPartnerOnWaitingList
                        : MailType.PartnerRegistrationFirstPartnerAccepted;
                }
                else
                {
                    mailToSend = isOnWaitingList
                        ? MailType.PartnerRegistrationMatchedOnWaitingList
                        : MailType.PartnerRegistrationMatchedAndAccepted;
                }
            }

            _serviceBusClient.SendMessage(new ComposeAndSendMailCommand
            {
                MailType = mailToSend,
                RegistrationId = registration.Id,
                Withhold = withhold,
                AllowDuplicate = false
            });

            return ownSeats;
        }
    }
}