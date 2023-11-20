using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Register;

public class PartnerRegistrationProcessor(PhoneNormalizer phoneNormalizer,
                                          IQueryable<QuestionOptionMapping> optionToRegistrableMappings,
                                          SpotManager spotManager,
                                          IRepository<Registration> registrations,
                                          PriceCalculator priceCalculator,
                                          CommandQueue commandQueue,
                                          IDateTimeProvider dateTimeProvider)
{
    public async Task<IEnumerable<Seat>> Process(Registration registration,
                                                 PartnerRegistrationProcessConfiguration config)
    {
        registration.RespondentFirstName = registration.Responses
                                                       .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_FirstName)
                                                       ?.ResponseString;
        registration.RespondentLastName = registration.Responses
                                                      .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_LastName)
                                                      ?.ResponseString;
        registration.RespondentEmail = registration.Responses
                                                   .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_Email)
                                                   ?.ResponseString;
        if (config.LanguageMappings != null)
        {
            registration.Language = config.LanguageMappings.FirstOrDefault(map => registration.Responses.Any(rsp => rsp.QuestionOptionId == map.QuestionOptionId))
                                          .Language;
        }

        if (config.QuestionId_Leader_Phone.HasValue)
        {
            registration.Phone = registration.Responses
                                             .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Leader_Phone.Value)
                                             ?.ResponseString;
            registration.PhoneNormalized = phoneNormalizer.NormalizePhone(registration.Phone);
        }

        var followerRegistration = new Registration
                                   {
                                       Id = Guid.NewGuid(),
                                       EventId = registration.EventId,
                                       ExternalTimestamp = registration.ExternalTimestamp,
                                       RespondentFirstName = registration.Responses
                                                                         .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_FirstName)
                                                                         ?.ResponseString,
                                       RespondentLastName = registration.Responses
                                                                        .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_LastName)
                                                                        ?.ResponseString,
                                       RespondentEmail = registration.Responses
                                                                     .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_Email)
                                                                     ?.ResponseString,
                                       ExternalIdentifier = registration.ExternalIdentifier,
                                       ReceivedAt = registration.ReceivedAt,
                                       RegistrationFormId = registration.RegistrationFormId,
                                       RegistrationId_Partner = registration.Id,
                                       Language = registration.Language,
                                       State = RegistrationState.Received
                                   };
        if (config.QuestionId_Follower_Phone.HasValue)
        {
            followerRegistration.Phone = registration.Responses
                                                     .FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Follower_Phone.Value)
                                                     ?.ResponseString;
            followerRegistration.PhoneNormalized = phoneNormalizer.NormalizePhone(followerRegistration.Phone);
        }

        registration.RegistrationId_Partner = followerRegistration.Id;

        var spots = new List<Seat>();

        var questionOptionIds = new HashSet<Guid>(registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue)
                                                              .Select(rsp => rsp.QuestionOptionId.Value));
        var roleSpecificRegistrableIds = config.RoleSpecificMappings?.Select(rsm => rsm.RegistrableId).ToHashSet();
        var registrables = await optionToRegistrableMappings
                                 .Where(map => map.Registrable.EventId == registration.EventId
                                            && (questionOptionIds.Contains(map.QuestionOptionId)
                                             || (roleSpecificRegistrableIds != null
                                              && map.RegistrableId != null
                                              && roleSpecificRegistrableIds.Contains(map.RegistrableId.Value))))
                                 .Include(map => map.Registrable)
                                 .Include(map => map.Registrable.Spots)
                                 .ToListAsync();
        foreach (var response in registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue))
        {
            foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
            {
                var seat = registrable.Registrable.MaximumDoubleSeats.HasValue
                               ? await spotManager.ReservePartnerSpot(registration.EventId,
                                                                      registrable.Registrable,
                                                                      registration.Id,
                                                                      followerRegistration.Id,
                                                                      true)
                               : await spotManager.ReserveSingleSpot(registration.EventId, registrable.Id, registration.Id, true);

                if (seat == null)
                {
                    registration.SoldOutMessage = (registration.SoldOutMessage == null
                                                       ? null
                                                       : registration.SoldOutMessage + Environment.NewLine)
                                                + string.Format(Properties.Resources.RegistrableSoldOut,
                                                                registrable.Registrable.DisplayName);
                }
                else
                {
                    spots.Add(seat);
                }
            }
        }

        if (config.RoleSpecificMappings != null)
        {
            foreach (var roleSpecificMapping in config.RoleSpecificMappings.Where(rsm => registration.Responses.Any(rsp => rsp.QuestionOptionId == rsm.QuestionOptionId)))
            {
                var registrable = registrables.First(rbl => rbl.RegistrableId == roleSpecificMapping.RegistrableId);
                var registrationId = roleSpecificMapping.Role == Role.Leader
                                         ? registration.Id
                                         : followerRegistration.Id;
                var seat = await spotManager.ReserveSingleSpot(registration.EventId, registrable.Id, registrationId,
                                                               false);
                if (seat == null)
                {
                    registration.SoldOutMessage = (registration.SoldOutMessage == null
                                                       ? null
                                                       : registration.SoldOutMessage + Environment.NewLine)
                                                + string.Format(Properties.Resources.RegistrableSoldOut,
                                                                registrable.Registrable.DisplayName);
                }
                else
                {
                    spots.Add(seat);
                }
            }
        }

        var isOnWaitingList = spots.Any(seat => seat.IsWaitingList);

        // finalize follower registration
        followerRegistration.IsOnWaitingList = isOnWaitingList;
        if (followerRegistration.IsOnWaitingList == false && followerRegistration.AdmittedAt == null)
        {
            followerRegistration.AdmittedAt = dateTimeProvider.Now;
        }

        var (totalFollower, admittedFollower, admittedAndReducedFollower, _, _, _, _) = await priceCalculator.CalculatePrice(followerRegistration, spots);
        followerRegistration.Price_Original = totalFollower;
        followerRegistration.Price_Admitted = admittedFollower;
        followerRegistration.Price_AdmittedAndReduced = admittedAndReducedFollower;

        await registrations.InsertOrUpdateEntity(followerRegistration);

        // finalize leader registration
        registration.IsOnWaitingList = isOnWaitingList;
        if (registration.IsOnWaitingList == false && registration.AdmittedAt == null)
        {
            registration.AdmittedAt = dateTimeProvider.Now;
        }

        var (total, admitted, admittedAndReduced, _, _, _, _) = await priceCalculator.CalculatePrice(registration, spots);
        registration.Price_Original = total;
        registration.Price_Admitted = admitted;
        registration.Price_AdmittedAndReduced = admittedAndReduced;

        await registrations.InsertOrUpdateEntity(registration);

        // send mail
        var mailType = isOnWaitingList
                           ? MailType.PartnerRegistrationMatchedOnWaitingList
                           : MailType.PartnerRegistrationMatchedAndAccepted;
        commandQueue.EnqueueCommand(new ComposeAndSendAutoMailCommand
                                    {
                                        EventId = registration.EventId,
                                        MailType = mailType,
                                        RegistrationId = registration.Id,
                                        AllowDuplicate = false
                                    });

        return spots;
    }
}