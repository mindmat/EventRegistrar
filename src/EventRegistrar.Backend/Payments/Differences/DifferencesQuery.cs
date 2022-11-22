using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Differences;

public class DifferencesQuery : IRequest<IEnumerable<DifferencesDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class DifferencesQueryHandler : IRequestHandler<DifferencesQuery, IEnumerable<DifferencesDisplayItem>>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Mail> _mails;

    public DifferencesQueryHandler(IQueryable<Registration> registrations,
                                   IQueryable<Mail> mails)
    {
        _registrations = registrations;
        _mails = mails;
    }

    public async Task<IEnumerable<DifferencesDisplayItem>> Handle(DifferencesQuery query,
                                                                  CancellationToken cancellationToken)
    {
        var differences = await _registrations.Where(reg => reg.EventId == query.EventId
                                                         && (reg.State == RegistrationState.Received || reg.State == RegistrationState.Paid)
                                                         && reg.IsOnWaitingList == false
                                                         && reg.Price_AdmittedAndReduced > 0m)
                                              .Select(reg => new
                                                             {
                                                                 Registration = reg,
                                                                 PaymentsTotal = reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                                        ? asn.Amount
                                                                                                                        : -asn.Amount),
                                                                 Difference = reg.Price_AdmittedAndReduced
                                                                            - reg.PaymentAssignments.Sum(asn => asn.PayoutRequestId == null
                                                                                                                    ? asn.Amount
                                                                                                                    : -asn.Amount)
                                                             })
                                              .Where(reg => reg.Difference != 0m
                                                         && reg.PaymentsTotal > 0m)
                                              .OrderBy(reg => reg.Registration.AdmittedAt)
                                              .Select(reg => new DifferencesDisplayItem
                                                             {
                                                                 RegistrationId = reg.Registration.Id,
                                                                 Price = reg.Registration.Price_AdmittedAndReduced,
                                                                 AmountPaid = reg.PaymentsTotal,
                                                                 Difference = reg.Difference,
                                                                 FirstName = reg.Registration.RespondentFirstName,
                                                                 LastName = reg.Registration.RespondentLastName,
                                                                 State = reg.Registration.State
                                                             })
                                              .ToListAsync(cancellationToken);

        var registrationIds = differences.Select(dif => dif.RegistrationId).ToList();
        var sentMails = await _mails.Where(mail => mail.EventId == query.EventId
                                                && mail.Registrations.Any(reg =>
                                                                              registrationIds.Contains(reg.RegistrationId))
                                                && (mail.Type == MailType.MoneyOwed || mail.Type == MailType.TooMuchPaid)
                                                && !mail.Discarded)
                                    .SelectMany(mail => mail.Registrations
                                                            .Where(map => registrationIds.Contains(map.RegistrationId))
                                                            .Select(map => new
                                                                           {
                                                                               map.RegistrationId,
                                                                               MailType = mail.Type.Value,
                                                                               mail.Created,
                                                                               mail.Sent,
                                                                               mail.DataJson
                                                                           }))
                                    .ToListAsync(cancellationToken);
        foreach (var registration in sentMails.GroupBy(mail => mail.RegistrationId)
                                              .Select(grp => new
                                                             {
                                                                 RegistrationId = grp.Key,
                                                                 Difference = differences.First(dif => dif.RegistrationId == grp.Key),
                                                                 LastPaymentDueMail = grp.Where(mail => mail.MailType == MailType.MoneyOwed)
                                                                                         .MaxBy(mail => mail.Sent ?? mail.Created),
                                                                 LastTooMuchPaidMail = grp.Where(mail => mail.MailType == MailType.TooMuchPaid)
                                                                                          .MaxBy(mail => mail.Sent ?? mail.Created)
                                                             }))
        {
            registration.Difference.PaymentDueMailSent = registration.LastPaymentDueMail?.Sent;
            registration.Difference.TooMuchPaidMailSent = registration.LastTooMuchPaidMail?.Sent;
        }

        return differences;
    }
}

public class DifferencesDisplayItem
{
    public Guid RegistrationId { get; set; }
    public decimal Price { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Difference { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public RegistrationState State { get; set; }
    public DateTimeOffset? PaymentDueMailSent { get; set; }
    public DateTimeOffset? TooMuchPaidMailSent { get; set; }
}