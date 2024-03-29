﻿using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.ReadableIds;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Register;

public class SingleRegistrationProcessor(PhoneNormalizer phoneNormalizer,
                                         SpotManager spotManager,
                                         PriceCalculator priceCalculator,
                                         IRepository<Registration> registrations,
                                         CommandQueue commandQueue,
                                         IQueryable<RegistrationForm> forms,
                                         IQueryable<Registrable> registrables,
                                         IDateTimeProvider dateTimeProvider,
                                         ReadableIdProvider readableIdProvider)
{
    public async Task<IEnumerable<Seat>> Process(Registration registration)
    {
        var form = await forms.Where(frm => frm.Id == registration.RegistrationFormId)
                              .Include(frm => frm.Questions!)
                              .ThenInclude(qst => qst.QuestionOptions!)
                              .ThenInclude(qop => qop.Mappings!)
                              .Include(frm => frm.MultiMappings)
                              .FirstOrDefaultAsync();
        if (form?.Questions == null)
        {
            return [];
        }

        var partnerRegistrableRequests = new List<PartnerRegistrableRequest>();
        Role? defaultRole = null;
        var soldOutRegistrableIds = new List<Guid>();
        var spots = new List<Seat>();
        foreach (var question in form.Questions)
        {
            var responses = registration.Responses!.Where(rsp => rsp.QuestionId == question.Id);
            foreach (var response in responses)
            {
                if (question.Mapping != null)
                {
                    switch (question.Mapping)
                    {
                        case QuestionMappingType.FirstName:
                            {
                                registration.RespondentFirstName = response.ResponseString;
                                break;
                            }
                        case QuestionMappingType.LastName:
                            {
                                registration.RespondentLastName = response.ResponseString;
                                break;
                            }
                        case QuestionMappingType.EMail:
                            {
                                registration.RespondentEmail = response.ResponseString;
                                break;
                            }
                        case QuestionMappingType.Phone:
                            {
                                registration.Phone = response.ResponseString;
                                registration.PhoneNormalized = phoneNormalizer.NormalizePhone(registration.Phone);
                                break;
                            }
                        case QuestionMappingType.Location:
                            {
                                registration.Location = response.ResponseString;
                                break;
                            }
                        case QuestionMappingType.Remarks:
                            {
                                if (string.IsNullOrWhiteSpace(response.ResponseString))
                                {
                                    break;
                                }

                                var text = $"{question.Section}: {response.ResponseString}";
                                if (string.IsNullOrEmpty(registration.Remarks))
                                {
                                    registration.Remarks = text;
                                }
                                else
                                {
                                    registration.Remarks += "\r\n" + text;
                                }

                                break;
                            }
                        case QuestionMappingType.Partner:
                            {
                                registration.PartnerOriginal = response.ResponseString;
                                if (string.IsNullOrWhiteSpace(registration.PartnerOriginal))
                                {
                                    registration.PartnerOriginal = null;
                                }

                                registration.PartnerNormalized = registration.PartnerOriginal?.ToLowerInvariant();
                                break;
                            }
                        case null:
                            break;
                    }
                }
                else if (response.QuestionOptionId != null)
                {
                    var questionOption = question.QuestionOptions!.FirstOrDefault(qop => qop.Id == response.QuestionOptionId);
                    if (questionOption?.Mappings == null)
                    {
                        continue;
                    }

                    foreach (var questionOptionMapping in questionOption.Mappings)
                    {
                        var role = await ProcessCombinedRegistrableId(registration,
                                                                      questionOptionMapping.Type,
                                                                      questionOptionMapping.RegistrableId,
                                                                      questionOptionMapping.Language,
                                                                      soldOutRegistrableIds,
                                                                      spots,
                                                                      partnerRegistrableRequests);
                        defaultRole ??= role;

                        if (questionOptionMapping.Type == MappingType.CanSwitchRole)
                        {
                            registration.CanSwitchRole = true;
                        }
                    }
                }
            }
        }

        var selectedQuestionOptionIds = registration.Responses!
                                                    .Select(rsp => rsp.QuestionOptionId)
                                                    .ToHashSet();
        foreach (var activatedMultiMapping in form.MultiMappings!.Where(mqm => mqm.QuestionOptionIds.All(qop => selectedQuestionOptionIds.Contains(qop))))
        {
            foreach (var registrableCombinedId in activatedMultiMapping.RegistrableCombinedIds)
            {
                var parsed = new CombinedMappingId(registrableCombinedId);
                var role = await ProcessCombinedRegistrableId(registration,
                                                              parsed.Type,
                                                              parsed.Id,
                                                              parsed.Language,
                                                              soldOutRegistrableIds,
                                                              spots,
                                                              partnerRegistrableRequests);
                defaultRole ??= role;
            }
        }


        // use default role
        var requestsWithoutRole = partnerRegistrableRequests.Where(prr => prr.Role == null).ToList();
        if (requestsWithoutRole.Any())
        {
            if (defaultRole == null)
            {
                var trackNames = await registrables.Where(rbl => requestsWithoutRole.Select(rwr => rwr.RegistrableId).Contains(rbl.Id))
                                                   .Select(rbl => rbl.DisplayName)
                                                   .ToListAsync();
                throw new InvalidOperationException(
                    $"Invalid mapping configuration: Mappings to partner registrable {trackNames.StringJoin()} but no role defined");
            }

            requestsWithoutRole.ForEach(rwr => rwr.Role = defaultRole);
        }

        registration.DefaultRole = defaultRole;

        // now book partner spots
        foreach (var partnerRegistrableRequest in partnerRegistrableRequests)
        {
            var ownIdentification = new RegistrationIdentification(registration);
            var spot = await spotManager.ReserveSinglePartOfPartnerSpot(registration.EventId,
                                                                        partnerRegistrableRequest.RegistrableId,
                                                                        registration.Id,
                                                                        ownIdentification,
                                                                        registration.PartnerNormalized,
                                                                        null,
                                                                        partnerRegistrableRequest.Role,
                                                                        true);
            if (spot == null)
            {
                soldOutRegistrableIds.Add(partnerRegistrableRequest.RegistrableId);
            }
            else
            {
                if (spot.IsPartnerSpot
                 && registration.RegistrationId_Partner == null)
                {
                    registration.RegistrationId_Partner = spot.GetOtherRegistrationId(registration.Id);
                    // set own id as partner id of partner registration
                    var partnerRegistration = await registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner);
                    if (partnerRegistration != null)
                    {
                        partnerRegistration.RegistrationId_Partner = registration.Id;
                    }
                }

                spots.Add(spot);
            }
        }


        if (soldOutRegistrableIds.Any())
        {
            var soldOutRegistrableNames = await registrables.Where(rbl => soldOutRegistrableIds.Contains(rbl.Id))
                                                            .Select(rbl => rbl.DisplayName)
                                                            .ToListAsync();
            registration.SoldOutMessage = soldOutRegistrableNames
                                          .Select(rbn => string.Format(Properties.Resources.RegistrableSoldOut, rbn))
                                          .StringJoin(Environment.NewLine);
            if (string.IsNullOrEmpty(registration.SoldOutMessage))
            {
                registration.SoldOutMessage = null;
            }
        }

        var (original, admitted, admittedAndReduced, _, _, isOnWaitingList, _) = await priceCalculator.CalculatePrice(registration, spots);
        registration.Price_Original = original;
        registration.Price_Admitted = admitted;
        registration.Price_AdmittedAndReduced = admittedAndReduced;

        registration.IsOnWaitingList = isOnWaitingList;
        if (registration is { IsOnWaitingList: false, AdmittedAt: null })
        {
            registration.AdmittedAt = dateTimeProvider.Now;
        }

        registration.ReadableIdentifier = await readableIdProvider.GetNextId(registration.EventId, GetInitials(registration));
        await registrations.InsertOrUpdateEntity(registration);

        // send mail
        var isPartnerRegistration = registration.PartnerNormalized != null;
        var isUnmatchedPartnerRegistration = isPartnerRegistration
                                          && spots.Any(spot => spot.PartnerEmail != null
                                                            && (spot.RegistrationId == null || spot.RegistrationId_Follower == null));
        MailType mailToSend;
        if (registration.Price_Admitted == 0m && !string.IsNullOrEmpty(registration.SoldOutMessage))
        {
            mailToSend = MailType.SoldOut;
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

        commandQueue.EnqueueCommand(new ComposeAndSendAutoMailCommand
                                    {
                                        EventId = registration.EventId,
                                        MailType = mailToSend,
                                        RegistrationId = registration.Id,
                                        AllowDuplicate = false
                                    });

        return spots;
    }

    private static string? GetInitials(Registration registration)
    {
        var initials = $"{registration.RespondentFirstName?.First()}{registration.RespondentLastName?.First()}";
        return string.IsNullOrWhiteSpace(initials)
                   ? null
                   : initials;
    }

    private async Task<Role?> ProcessCombinedRegistrableId(Registration registration,
                                                           MappingType? mappingType,
                                                           Guid? registrableId,
                                                           string? language,
                                                           ICollection<Guid> soldOutRegistrableIds,
                                                           ICollection<Seat> spots,
                                                           ICollection<PartnerRegistrableRequest> partnerRegistrableRequests)
    {
        Role? defaultRole = null;
        switch (mappingType)
        {
            case MappingType.SingleRegistrable:
                {
                    if (registrableId != null)
                    {
                        var spot = await spotManager.ReserveSingleSpot(registration.EventId,
                                                                       registrableId.Value,
                                                                       registration.Id,
                                                                       true);
                        if (spot == null)
                        {
                            soldOutRegistrableIds.Add(registrableId.Value);
                        }
                        else
                        {
                            spots.Add(spot);
                        }
                    }

                    break;
                }
            case MappingType.PartnerRegistrableLeader:
                {
                    if (registrableId != null)
                    {
                        partnerRegistrableRequests.Add(new PartnerRegistrableRequest
                                                       {
                                                           RegistrableId = registrableId.Value,
                                                           Role = Role.Leader
                                                       });
                    }

                    break;
                }

            case MappingType.PartnerRegistrableFollower:
                {
                    if (registrableId != null)
                    {
                        partnerRegistrableRequests.Add(new PartnerRegistrableRequest
                                                       {
                                                           RegistrableId = registrableId.Value,
                                                           Role = Role.Follower
                                                       });
                    }

                    break;
                }

            case MappingType.PartnerRegistrable:
                {
                    if (registrableId != null)
                    {
                        partnerRegistrableRequests.Add(new PartnerRegistrableRequest
                                                       {
                                                           RegistrableId = registrableId.Value
                                                       });
                    }

                    break;
                }

            case MappingType.Language:
                {
                    registration.Language = language;
                    break;
                }

            case MappingType.RoleLeader:
                {
                    defaultRole = Role.Leader;
                    break;
                }

            case MappingType.RoleFollower:
                {
                    defaultRole = Role.Follower;
                    break;
                }
        }

        return defaultRole;
    }
}

internal class PartnerRegistrableRequest
{
    public Guid RegistrableId { get; set; }
    public Role? Role { get; set; }
}