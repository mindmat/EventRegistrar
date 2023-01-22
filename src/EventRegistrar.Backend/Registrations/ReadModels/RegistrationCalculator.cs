﻿using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Register;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.ReadModels;

public class RegistrationCalculator : ReadModelCalculator<RegistrationDisplayItem>
{
    public override string QueryName => nameof(RegistrationQuery);
    public override bool IsDateDependent => false;

    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Seat> _spots;
    private readonly IQueryable<PaymentAssignment> _assignments;
    private readonly EnumTranslator _enumTranslator;

    public RegistrationCalculator(IQueryable<Registration> registrations,
                                  IQueryable<Seat> spots,
                                  EnumTranslator enumTranslator,
                                  IQueryable<PaymentAssignment> assignments)
    {
        _registrations = registrations;
        _spots = spots;
        _enumTranslator = enumTranslator;
        _assignments = assignments;
    }

    public override async Task<RegistrationDisplayItem> CalculateTyped(Guid eventId, Guid? registrationId, CancellationToken cancellationToken)
    {
        var content = await _registrations.Where(reg => reg.EventId == eventId
                                                     && reg.Id == registrationId)
                                          .Select(reg => new RegistrationDisplayItem
                                                         {
                                                             Id = reg.Id,
                                                             IsWaitingList = reg.IsOnWaitingList,
                                                             Price = reg.Price_AdmittedAndReduced,
                                                             Status = reg.State,
                                                             Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                                     ? asn.Amount
                                                                                                                     : -asn.Amount)
                                                                 ?? 0m,
                                                             Language = reg.Language,
                                                             ReceivedAt = reg.ReceivedAt,
                                                             ReminderLevel = reg.ReminderLevel,
                                                             Remarks = reg.Remarks,
                                                             RemarksProcessed = reg.RemarksProcessed,
                                                             Email = reg.RespondentEmail,
                                                             FirstName = reg.RespondentFirstName,
                                                             LastName = reg.RespondentLastName,
                                                             SoldOutMessage = reg.SoldOutMessage,
                                                             FallbackToPartyPass = reg.FallbackToPartyPass,
                                                             SmsCount = reg.Sms!.Count,
                                                             PhoneNormalized = reg.PhoneNormalized,
                                                             PhoneFormatted = reg.PhoneNormalized,
                                                             PartnerOriginal = reg.PartnerNormalized == null
                                                                                   ? null
                                                                                   : reg.PartnerOriginal,
                                                             PartnerName = reg.RegistrationId_Partner == null
                                                                               ? null
                                                                               : $"{reg.Registration_Partner!.RespondentFirstName} {reg.Registration_Partner.RespondentLastName}",
                                                             PartnerId = reg.RegistrationId_Partner,
                                                             WillPayAtCheckin = reg.WillPayAtCheckin,
                                                             InternalNotes = reg.InternalNotes,
                                                             PricePackageId_ManualFallback = reg.PricePackageId_ManualFallback,
                                                             Reductions = reg.IndividualReductions!.Select(ird => new IndividualReductionDisplayItem
                                                                                                                  {
                                                                                                                      Id = ird.Id,
                                                                                                                      Type = ird.Type,
                                                                                                                      Amount = ird.Amount,
                                                                                                                      Reason = ird.Reason
                                                                                                                  }),
                                                             Mails = reg.Mails!
                                                                        .Where(mir => !mir.Mail!.Discarded)
                                                                        .OrderByDescending(mir => mir.Mail!.Created)
                                                                        .Select(mir => new MailDisplayItem
                                                                                       {
                                                                                           Type = mir.Mail!.BulkMailKey == null
                                                                                                      ? MailDisplayType.Auto
                                                                                                      : MailDisplayType.Bulk,
                                                                                           MailId = mir.MailId,
                                                                                           Subject = mir.Mail.Subject,
                                                                                           Created = mir.Mail.Created,
                                                                                           Withhold = mir.Mail.Withhold,
                                                                                           SentAt = mir.Mail.Sent,
                                                                                           State = mir.State
                                                                                       }),
                                                             ImportedMails = reg.ImportedMails!
                                                                                .OrderByDescending(mir => mir.Mail!.Date)
                                                                                .Select(mir => new MailDisplayItem
                                                                                               {
                                                                                                   Type = MailDisplayType.Imported,
                                                                                                   MailId = mir.ImportedMailId,
                                                                                                   Subject = mir.Mail!.Subject,
                                                                                                   Created = mir.Mail.Date
                                                                                               })
                                                         })
                                          .FirstAsync(cancellationToken);

        content.Spots = await _spots.Where(spot => (spot.Registration!.EventId == eventId
                                                 && spot.RegistrationId == registrationId)
                                                || (spot.Registration_Follower!.EventId == eventId
                                                 && spot.RegistrationId_Follower == registrationId))
                                    .Where(spot => !spot.IsCancelled)
                                    .OrderByDescending(spot => spot.Registrable!.IsCore)
                                    .ThenBy(spot => spot.Registrable!.ShowInMailListOrder)
                                    .Select(spot => new SpotDisplayItem
                                                    {
                                                        Id = spot.Id,
                                                        RegistrableId = spot.RegistrableId,
                                                        RegistrableName = spot.Registrable!.Name,
                                                        RegistrableNameSecondary = spot.Registrable.NameSecondary,
                                                        PartnerRegistrationId = spot.IsPartnerSpot
                                                                                    ? spot.RegistrationId == registrationId
                                                                                          ? spot.RegistrationId_Follower
                                                                                          : spot.RegistrationId
                                                                                    : null,
                                                        FirstPartnerJoined = spot.FirstPartnerJoined,
                                                        IsCore = spot.Registrable.IsCore,
                                                        RoleText = spot.Registrable.Type == RegistrableType.Double
                                                                       ? _enumTranslator.Translate(spot.RegistrationId == registrationId
                                                                                                       ? Role.Leader
                                                                                                       : Role.Follower)
                                                                       : null,
                                                        PartnerName = spot.IsPartnerSpot
                                                                          ? spot.RegistrationId == registrationId
                                                                                ? $"{spot.Registration_Follower!.RespondentFirstName} {spot.Registration_Follower.RespondentLastName}"
                                                                                : $"{spot.Registration!.RespondentFirstName} {spot.Registration.RespondentLastName}"
                                                                          : null,
                                                        IsWaitingList = spot.IsWaitingList,
                                                        Type = spot.Registrable.Type
                                                    })
                                    .ToListAsync(cancellationToken);

        var dataAssignments = await _assignments.Where(ass => ass.Registration!.EventId == eventId
                                                           && ass.RegistrationId == registrationId
                                                           && ass.PaymentAssignmentId_Counter == null)
                                                .Select(ass => new
                                                               {
                                                                   ass.Id,
                                                                   ass.Amount,
                                                                   ass.IncomingPaymentId,
                                                                   ass.OutgoingPaymentId,
                                                                   Currency_Incoming = ass.IncomingPayment!.Payment!.Currency,
                                                                   BookingDate_Incoming = (DateTime?)ass.IncomingPayment.Payment.BookingDate,
                                                                   Currency_Outgoing = ass.OutgoingPayment!.Payment!.Currency,
                                                                   BookingDate_Outgoing = (DateTime?)ass.OutgoingPayment.Payment.BookingDate
                                                               })
                                                .ToListAsync(cancellationToken);

        content.Payments = dataAssignments.Where(ass => ass.IncomingPaymentId != null)
                                          .Select(ass => new AssignedPaymentDisplayItem
                                                         {
                                                             PaymentAssignmentId = ass.Id,
                                                             Amount = ass.Amount,
                                                             Currency = ass.Currency_Incoming,
                                                             BookingDate = ass.BookingDate_Incoming!.Value
                                                         })
                                          .Concat(dataAssignments.Where(ass => ass.OutgoingPaymentId != null)
                                                                 .Select(ass => new AssignedPaymentDisplayItem
                                                                                {
                                                                                    PaymentAssignmentId = ass.Id,
                                                                                    Amount = -ass.Amount,
                                                                                    Currency = ass.Currency_Outgoing,
                                                                                    BookingDate = ass.BookingDate_Outgoing!.Value
                                                                                }))
                                          .ToList();
        return content;
    }
}

public class UpdateRegistrationWhenOutgoingPaymentAssigned : IEventToCommandTranslation<RegistrationProcessed>,
                                                             IEventToCommandTranslation<RegistrationCancelled>,
                                                             IEventToCommandTranslation<SpotRemoved>,
                                                             IEventToCommandTranslation<SpotAdded>,
                                                             IEventToCommandTranslation<OutgoingPaymentAssigned>,
                                                             IEventToCommandTranslation<OutgoingPaymentUnassigned>,
                                                             IEventToCommandTranslation<IncomingPaymentUnassigned>,
                                                             IEventToCommandTranslation<IncomingPaymentAssigned>,
                                                             IEventToCommandTranslation<PriceChanged>,
                                                             IEventToCommandTranslation<ImportedMailAssigned>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateRegistrationWhenOutgoingPaymentAssigned(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public IEnumerable<IRequest> Translate(RegistrationProcessed e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId);
        }
    }

    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId);
        }
    }

    public IEnumerable<IRequest> Translate(SpotAdded e)
    {
        if (!e.IsInitialProcessing)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId);
        }
    }

    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e.Reason == RemoveSpotReason.Modification)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId);
        }
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId!.Value);
        }
    }

    public IEnumerable<IRequest> Translate(OutgoingPaymentUnassigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId!.Value);
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId!.Value);
        }
    }

    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId!.Value);
        }
    }

    public IEnumerable<IRequest> Translate(ImportedMailAssigned e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId);
        }
    }

    public IEnumerable<IRequest> Translate(PriceChanged e)
    {
        if (e.EventId != null)
        {
            yield return CreateUpdateCommand(e.EventId!.Value, e.RegistrationId);
        }
    }

    private UpdateReadModelCommand CreateUpdateCommand(Guid eventId, Guid registrationId)
    {
        return new UpdateReadModelCommand
               {
                   QueryName = nameof(RegistrationQuery),
                   EventId = eventId,
                   RowId = registrationId,
                   DirtyMoment = _dateTimeProvider.Now
               };
    }
}