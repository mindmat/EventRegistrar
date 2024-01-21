using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.ManualTrigger;

public class PossibleMailTypesQuery : IRequest<IEnumerable<MailTypeItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class PossibleMailTypesQueryHandler(IQueryable<BulkMailTemplate> mailTemplates,
                                           IQueryable<Event> events,
                                           EnumTranslator enumTranslator,
                                           MailConfiguration mailConfiguration,
                                           ReadModelReader readModelReader)
    : IRequestHandler<PossibleMailTypesQuery, IEnumerable<MailTypeItem>>
{
    public async Task<IEnumerable<MailTypeItem>> Handle(PossibleMailTypesQuery query,
                                                        CancellationToken cancellationToken)
    {
        var registration = await readModelReader.GetDeserialized<RegistrationDisplayItem>(nameof(RegistrationQuery),
                                                                                          query.EventId,
                                                                                          query.RegistrationId,
                                                                                          cancellationToken);
        var partnerRegistration = registration.PartnerId == null
                                      ? null
                                      : await readModelReader.GetDeserialized<RegistrationDisplayItem>(nameof(RegistrationQuery),
                                                                                                       query.EventId,
                                                                                                       query.RegistrationId,
                                                                                                       cancellationToken);
        var eventState = await events.Where(evt => evt.Id == query.EventId)
                                     .Select(evt => evt.State)
                                     .FirstAsync(cancellationToken);
        var possibleMailTypes = GetPossibleMailTypes(registration,
                                                     partnerRegistration,
                                                     eventState == EventState.Setup);

        var activeBulkMails = await mailTemplates.Where(tpl => tpl.EventId == query.EventId
                                                            && tpl.Discarded == false
                                                            && tpl.Mails!.Any())
                                                 .Select(tpl => new MailTypeItem
                                                                {
                                                                    BulkMailKey = tpl.BulkMailKey,
                                                                    UserText = tpl.Subject
                                                                })
                                                 .ToListAsync(cancellationToken);

        return possibleMailTypes.Select(typ => new MailTypeItem
                                               {
                                                   Type = typ,
                                                   UserText = enumTranslator.Translate(typ)
                                               })
                                .Union(activeBulkMails);
    }

    private IEnumerable<MailType> GetPossibleMailTypes(RegistrationDisplayItem registration,
                                                       RegistrationDisplayItem? partnerRegistration,
                                                       bool isInSetupPhase)
    {
        if (registration.Status == RegistrationState.Cancelled || isInSetupPhase)
        {
            yield return MailType.RegistrationCancelled;
        }

        if ((registration.Price == 0m && !string.IsNullOrEmpty(registration.SoldOutMessage)) || isInSetupPhase)
        {
            yield return MailType.SoldOut;
        }

        if (registration.IsWaitingList == true || isInSetupPhase)
        {
            yield return MailType.OptionsForRegistrationsOnWaitingList;
        }

        var isPartnerRegistration = registration.IsPartnerRegistration;
        if (mailConfiguration.PartnerRegistrationPossible && (isPartnerRegistration || isInSetupPhase))
        {
            if (registration.PartnerId == null)
            {
                yield return registration.IsWaitingList == true
                                 ? MailType.PartnerRegistrationFirstPartnerOnWaitingList
                                 : MailType.PartnerRegistrationFirstPartnerAccepted;
            }

            var paidCount = (registration.Status == RegistrationState.Paid ? 1 : 0)
                          + (partnerRegistration?.Status == RegistrationState.Paid ? 1 : 0);

            if (paidCount == 0)
            {
                yield return registration.IsWaitingList == true
                                 ? MailType.PartnerRegistrationMatchedOnWaitingList
                                 : MailType.PartnerRegistrationMatchedAndAccepted;
            }

            if (paidCount == 1)
            {
                yield return MailType.PartnerRegistrationFirstPaid;
            }

            if (paidCount == 2)
            {
                yield return MailType.PartnerRegistrationFullyPaid;
            }

            var dueAmount = (registration.Price ?? 0m) - registration.Paid;
            if (dueAmount > 0 || isInSetupPhase)
            {
                yield return MailType.PartnerRegistrationFirstReminder;
                yield return MailType.PartnerRegistrationSecondReminder;
            }
        }

        if (mailConfiguration.SingleRegistrationPossible && (!isPartnerRegistration || isInSetupPhase))
        {
            if (registration.IsWaitingList == true || isInSetupPhase)
            {
                yield return MailType.SingleRegistrationOnWaitingList;
            }

            if (registration.Status == RegistrationState.Paid || isInSetupPhase)
            {
                yield return MailType.SingleRegistrationFullyPaid;
            }

            yield return MailType.SingleRegistrationAccepted;

            var dueAmount = (registration.Price ?? 0m) - registration.Paid;
            if (dueAmount > 0 || isInSetupPhase)
            {
                yield return MailType.SingleRegistrationFirstReminder;
                yield return MailType.SingleRegistrationSecondReminder;
            }

            if (isInSetupPhase)
            {
                yield return MailType.RegistrationReceived;
                yield return MailType.MoneyOwed;
                yield return MailType.TooMuchPaid;
            }
        }
    }
}

public class MailTypeItem
{
    public string? BulkMailKey { get; set; }
    public MailType? Type { get; set; }
    public string? UserText { get; set; }
}