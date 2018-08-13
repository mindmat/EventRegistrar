using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Seats;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class CoupleRegistrationProcessor
    {
        private readonly IQueryable<QuestionOptionToRegistrableMapping> _optionToRegistrableMappings;
        private readonly PhoneNormalizer _phoneNormalizer;
        private readonly PriceCalculator _priceCalculator;
        private readonly IRepository<Registration> _registrations;
        private readonly SeatManager _seatManager;

        public CoupleRegistrationProcessor(PhoneNormalizer phoneNormalizer,
                                           IQueryable<QuestionOptionToRegistrableMapping> optionToRegistrableMappings,
                                           SeatManager seatManager,
                                           IRepository<Registration> registrations,
                                           PriceCalculator priceCalculator)
        {
            _phoneNormalizer = phoneNormalizer;
            _optionToRegistrableMappings = optionToRegistrableMappings;
            _seatManager = seatManager;
            _registrations = registrations;
            _priceCalculator = priceCalculator;
        }

        public async Task<IEnumerable<Seat>> Process(Registration registration, CoupleRegistrationProcessConfiguration config)
        {
            registration.RespondentFirstName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_FirstName)?.ResponseString;
            registration.RespondentLastName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_LastName)?.ResponseString;
            registration.RespondentEmail = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_Email)?.ResponseString;
            if (config.QuestionId_Leader_Phone.HasValue)
            {
                registration.Phone = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_Phone.Value)?.ResponseString;
                registration.PhoneNormalized = _phoneNormalizer.NormalizePhone(registration.Phone);
            }

            var followerRegistration = new Registration
            {
                Id = Guid.NewGuid(),
                EventId = registration.EventId,
                ExternalTimestamp = registration.ExternalTimestamp,
                RespondentFirstName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_FirstName)?.ResponseString,
                RespondentLastName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_LastName)?.ResponseString,
                RespondentEmail = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_Email)?.ResponseString,
                ExternalIdentifier = registration.ExternalIdentifier,
                ReceivedAt = registration.ReceivedAt,
                RegistrationFormId = registration.RegistrationFormId,
                RegistrationId_Partner = registration.Id,
                State = RegistrationState.Received,
            };
            if (config.QuestionId_Follower_Phone.HasValue)
            {
                followerRegistration.Phone = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_Phone.Value)?.ResponseString;
                followerRegistration.PhoneNormalized = _phoneNormalizer.NormalizePhone(followerRegistration.Phone);
            }

            registration.RegistrationId_Partner = followerRegistration.Id;

            var spots = new List<Seat>();

            var questionOptionIds = new HashSet<Guid>(registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value));
            var registrables = await _optionToRegistrableMappings
                                            .Where(map => map.Registrable.EventId == registration.EventId
                                                       && questionOptionIds.Contains(map.QuestionOptionId))
                                            .Include(map => map.Registrable)
                                            .Include(map => map.Registrable.Seats)
                                            .ToListAsync();
            foreach (var response in registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue))
            {
                foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
                {
                    var seat = _seatManager.ReservePartnerSpot(registration.EventId, registrable.Registrable, registration.Id, followerRegistration.Id);
                    if (seat == null)
                    {
                        registration.SoldOutMessage = (registration.SoldOutMessage == null ? string.Empty : registration.SoldOutMessage + Environment.NewLine) +
                                                      string.Format(Resources.RegistrableSoldOut, registrable.Registrable.Name);
                    }
                    else
                    {
                        spots.Add(seat);
                    }
                }
            }

            followerRegistration.IsWaitingList = spots.Any(seat => seat.IsWaitingList);
            if (followerRegistration.IsWaitingList == false && !followerRegistration.AdmittedAt.HasValue)
            {
                followerRegistration.AdmittedAt = DateTime.UtcNow;
            }

            followerRegistration.Price = await _priceCalculator.CalculatePrice(registration.Responses, spots);

            await _registrations.InsertOrUpdateEntity(followerRegistration);

            return spots;
        }
    }
}