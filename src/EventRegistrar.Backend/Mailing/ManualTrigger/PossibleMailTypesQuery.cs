using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.ManualTrigger;

public class PossibleMailTypesQuery : IRequest<IEnumerable<MailTypeItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class PossibleMailTypesQueryHandler : IRequestHandler<PossibleMailTypesQuery, IEnumerable<MailTypeItem>>
{
    private readonly IQueryable<BulkMailTemplate> _mailTemplates;
    private readonly EnumTranslator _enumTranslator;
    private readonly MailConfiguration _mailConfiguration;
    private readonly IQueryable<Registration> _registrations;

    public PossibleMailTypesQueryHandler(IQueryable<Registration> registrations,
                                         IQueryable<BulkMailTemplate> mailTemplates,
                                         EnumTranslator enumTranslator,
                                         MailConfiguration mailConfiguration)
    {
        _registrations = registrations;
        _mailTemplates = mailTemplates;
        _enumTranslator = enumTranslator;
        _mailConfiguration = mailConfiguration;
    }

    public async Task<IEnumerable<MailTypeItem>> Handle(PossibleMailTypesQuery query,
                                                        CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == query.RegistrationId
                                                          && reg.EventId == query.EventId)
                                               .Include(reg => reg.Event)
                                               .FirstAsync(cancellationToken);
        var partnerRegistration = registration.RegistrationId_Partner == null
                                      ? null
                                      : await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner
                                                                       && reg.EventId == query.EventId)
                                                            .FirstAsync(cancellationToken);

        var possibleMailTypes = GetPossibleMailTypes(registration, partnerRegistration, registration.Event!.State == EventState.Setup);

        var activeBulkMails = await _mailTemplates.Where(tpl => tpl.EventId == query.EventId
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
                                                   UserText = _enumTranslator.Translate(typ)
                                               })
                                .Union(activeBulkMails);
    }

    private IEnumerable<MailType> GetPossibleMailTypes(Registration registration,
                                                       Registration? partnerRegistration,
                                                       bool isInSetupPhase)
    {
        if (registration.State == RegistrationState.Cancelled || isInSetupPhase)
        {
            yield return MailType.RegistrationCancelled;
        }

        if ((registration.Price_Admitted == 0m && !string.IsNullOrEmpty(registration.SoldOutMessage)) || isInSetupPhase)
        {
            yield return MailType.SoldOut;
        }

        if (_mailConfiguration.PartnerRegistrationPossible && (registration.IsParterRegistration() || isInSetupPhase))
        {
            if (registration.RegistrationId_Partner == null)
            {
                yield return registration.IsOnWaitingList == true
                                 ? MailType.PartnerRegistrationFirstPartnerOnWaitingList
                                 : MailType.PartnerRegistrationFirstPartnerAccepted;
            }

            var paidCount = (registration.State == RegistrationState.Paid ? 1 : 0)
                          + (partnerRegistration?.State == RegistrationState.Paid ? 1 : 0);

            if (paidCount == 0)
            {
                yield return registration.IsOnWaitingList == true
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

            if (isInSetupPhase)
            {
                yield return MailType.PartnerRegistrationFirstReminder;
                yield return MailType.PartnerRegistrationSecondReminder;
            }
        }
        else if (_mailConfiguration.SingleRegistrationPossible)
        {
            if (registration.IsOnWaitingList == true || isInSetupPhase)
            {
                yield return MailType.SingleRegistrationOnWaitingList;
                yield return MailType.OptionsForRegistrationsOnWaitingList;
            }

            if (registration.State == RegistrationState.Paid || isInSetupPhase)
            {
                yield return MailType.SingleRegistrationFullyPaid;
            }

            yield return MailType.SingleRegistrationAccepted;

            if (isInSetupPhase)
            {
                yield return MailType.RegistrationReceived;
                yield return MailType.SingleRegistrationFirstReminder;
                yield return MailType.SingleRegistrationSecondReminder;
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