using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.ManualTrigger;

public class PossibleMailTypesQuery : IRequest<IEnumerable<MailTypeItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class PossibleMailTypesQueryHandler : IRequestHandler<PossibleMailTypesQuery, IEnumerable<MailTypeItem>>
{
    private readonly IQueryable<MailTemplate> _mailTemplates;
    private readonly IQueryable<Registration> _registrations;

    public PossibleMailTypesQueryHandler(IQueryable<Registration> registrations,
                                         IQueryable<MailTemplate> mailTemplates)
    {
        _registrations = registrations;
        _mailTemplates = mailTemplates;
    }

    public async Task<IEnumerable<MailTypeItem>> Handle(PossibleMailTypesQuery query,
                                                        CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == query.RegistrationId
                                                          && reg.EventId == query.EventId)
                                               .FirstAsync(cancellationToken);
        var partnerRegistration = registration.RegistrationId_Partner == null
                                      ? null
                                      : await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner
                                                                       && reg.EventId == query.EventId)
                                                            .FirstAsync(cancellationToken);

        var possibleMailTypes = GetPossibleMailTypes(registration, partnerRegistration);

        var activeBulkMails = await _mailTemplates.Where(tpl => tpl.EventId == query.EventId
                                                             && tpl.BulkMailKey != null
                                                             && !tpl.IsDeleted
                                                             && tpl.Mails!.Any())
                                                  .Select(tpl =>
                                                              new MailTypeItem
                                                              { BulkMailKey = tpl.BulkMailKey, UserText = tpl.Subject })
                                                  .ToListAsync(cancellationToken);

        return possibleMailTypes.Select(typ => new MailTypeItem
                                               {
                                                   Type = typ,
                                                   UserText = Properties.Resources.ResourceManager.GetString($"MailType_{typ}") ?? typ.ToString()
                                               })
                                .Union(activeBulkMails);
    }

    private static IEnumerable<MailType> GetPossibleMailTypes(Registration registration,
                                                              Registration partnerRegistration)
    {
        if (registration.State == RegistrationState.Cancelled)
        {
            yield return MailType.RegistrationCancelled;
        }

        if (registration.Price_Admitted == 0m && !string.IsNullOrEmpty(registration.SoldOutMessage))
        {
            yield return MailType.SoldOut;
        }

        if (registration.IsParterRegistration())
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
        }
        else
        {
            if (registration.IsOnWaitingList == true)
            {
                yield return MailType.SingleRegistrationOnWaitingList;
                yield return MailType.OptionsForRegistrationsOnWaitingList;
            }

            if (registration.State == RegistrationState.Paid)
            {
                yield return MailType.SingleRegistrationFullyPaid;
            }

            yield return MailType.SingleRegistrationAccepted;
        }
    }
}

public class MailTypeItem
{
    public string? BulkMailKey { get; set; }
    public MailType? Type { get; set; }
    public string UserText { get; set; } = null!;
}