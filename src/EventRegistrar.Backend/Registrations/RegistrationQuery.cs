using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Register;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationQuery : IRequest<SerializedJson<RegistrationDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RegistrationQueryHandler(ReadModelReader readModelReader) : IRequestHandler<RegistrationQuery, SerializedJson<RegistrationDisplayItem>>
{
    public async Task<SerializedJson<RegistrationDisplayItem>> Handle(RegistrationQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<RegistrationDisplayItem>(nameof(RegistrationQuery),
                                                                  query.EventId,
                                                                  query.RegistrationId,
                                                                  cancellationToken);
    }
}

public class RegistrationCalculator(IQueryable<Registration> registrations,
                                    IQueryable<Seat> spots,
                                    EnumTranslator enumTranslator,
                                    IQueryable<PaymentAssignment> assignments)
    : ReadModelCalculator<RegistrationDisplayItem>
{
    public override string QueryName => nameof(RegistrationQuery);
    public override bool IsDateDependent => false;

    public override async Task<RegistrationDisplayItem> CalculateTyped(Guid eventId, Guid? registrationId, CancellationToken cancellationToken)
    {
        var content = await registrations.Where(reg => reg.EventId == eventId
                                                    && reg.Id == registrationId)
                                         .Select(reg => new RegistrationDisplayItem
                                                        {
                                                            Id = reg.Id,
                                                            ReadableId = reg.ReadableIdentifier,
                                                            IsWaitingList = reg.IsOnWaitingList != false,
                                                            Price = reg.Price_AdmittedAndReduced,
                                                            Status = reg.State,
                                                            Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.OutgoingPayment == null
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
                                                            Location = reg.Location,
                                                            SoldOutMessage = reg.SoldOutMessage,
                                                            FallbackToPartyPass = reg.FallbackToPartyPass,
                                                            SmsCount = reg.Sms!.Count,
                                                            PhoneNormalized = reg.PhoneNormalized,
                                                            PhoneFormatted = reg.PhoneNormalized,
                                                            IsPartnerRegistration = reg.IsPartnerRegistration(),
                                                            PartnerOriginal = reg.PartnerNormalized == null
                                                                                  ? null
                                                                                  : reg.PartnerOriginal,
                                                            PartnerName = reg.RegistrationId_Partner == null
                                                                              ? null
                                                                              : $"{reg.Registration_Partner!.RespondentFirstName} {reg.Registration_Partner.RespondentLastName}",
                                                            PartnerId = reg.RegistrationId_Partner,
                                                            WillPayAtCheckin = reg.WillPayAtCheckin,
                                                            InternalNotes = reg.InternalNotes,
                                                            PricePackageIds_ManualFallback = reg.PricePackageIds_ManualFallback ?? Enumerable.Empty<Guid>(),
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
                                                                       .Select(mir => new MailMetadata
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
                                                                               .Select(mir => new MailMetadata
                                                                                              {
                                                                                                  Type = MailDisplayType.Imported,
                                                                                                  MailId = mir.ImportedMailId,
                                                                                                  Subject = mir.Mail!.Subject,
                                                                                                  Created = mir.Mail.Date
                                                                                              })
                                                        })
                                         .FirstAsync(cancellationToken);

        content.Spots = await spots.Where(spot => (spot.Registration!.EventId == eventId
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
                                                                      ? enumTranslator.Translate(spot.RegistrationId == registrationId
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

        var dataAssignments = await assignments.Where(ass => ass.Registration!.EventId == eventId
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

public class UpdateRegistrationWhenOutgoingPaymentAssigned(IDateTimeProvider dateTimeProvider) : IEventToCommandTranslation<RegistrationProcessed>,
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
                   DirtyMoment = dateTimeProvider.Now
               };
    }
}