using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Differences;

public class DifferencesQuery : IRequest<IEnumerable<DifferencesDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class DifferencesQueryHandler : IRequestHandler<DifferencesQuery, IEnumerable<DifferencesDisplayItem>>
{
    private readonly IQueryable<Registration> _registrations;

    public DifferencesQueryHandler(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<IEnumerable<DifferencesDisplayItem>> Handle(DifferencesQuery query,
                                                                  CancellationToken cancellationToken)
    {
        var differences = await _registrations.Where(reg => reg.EventId == query.EventId
                                                         && (reg.State == RegistrationState.Received || reg.State == RegistrationState.Paid)
                                                         && reg.IsOnWaitingList == false
                                                         && reg.Price_AdmittedAndReduced > 0m
                                                         && !reg.WillPayAtCheckin)
                                              .Select(reg => new
                                                             {
                                                                 Registration = reg,
                                                                 PaymentsTotal = reg.PaymentAssignments!
                                                                                    .Where(asn => asn.IncomingPaymentId != null)
                                                                                    .Sum(asn => asn.Amount),
                                                                 AmountRepaid = reg.PaymentAssignments!
                                                                                   .Where(asn => asn.OutgoingPaymentId != null)
                                                                                   .Sum(asn => asn.Amount),
                                                                 AmountPayoutRequests = reg.PayoutRequests!
                                                                                           .Where(rpy => rpy.State != PayoutState.Confirmed)
                                                                                           .Sum(rpy => rpy.Amount),
                                                                 Mails = reg.Mails!.Select(mtr => mtr.Mail!)
                                                                            .Where(mail => !mail.Discarded
                                                                                        && (mail.Type == MailType.MoneyOwed
                                                                                         || mail.Type == MailType.TooMuchPaid))
                                                                            .Select(mail => new
                                                                                            {
                                                                                                mail.Type,
                                                                                                mail.Created,
                                                                                                mail.Sent
                                                                                            })
                                                             })
                                              .Where(reg => reg.PaymentsTotal > 0m)
                                              .OrderBy(reg => reg.Registration.AdmittedAt)
                                              .ToListAsync(cancellationToken);

        return differences.Select(reg => new DifferencesDisplayItem
                                         {
                                             RegistrationId = reg.Registration.Id,
                                             Price = reg.Registration.Price_AdmittedAndReduced,
                                             AmountPaid = reg.PaymentsTotal,
                                             AmountRepaid = reg.AmountRepaid + reg.AmountPayoutRequests,
                                             Difference = reg.Registration.Price_AdmittedAndReduced - reg.PaymentsTotal + reg.AmountRepaid + reg.AmountPayoutRequests,
                                             FirstName = reg.Registration.RespondentFirstName,
                                             LastName = reg.Registration.RespondentLastName,
                                             State = reg.Registration.State,
                                             InternalNotes = reg.Registration.InternalNotes,
                                             PaymentDueMailSent = reg.Mails.Where(mail => mail.Type == MailType.MoneyOwed)
                                                                     .MaxBy(mail => mail.Sent ?? mail.Created)
                                                                     ?.Sent,
                                             TooMuchPaidMailSent = reg.Mails.Where(mail => mail.Type == MailType.TooMuchPaid)
                                                                      .MaxBy(mail => mail.Sent ?? mail.Created)
                                                                      ?.Sent
                                         })
                          .Where(reg => reg.Difference != 0);
    }
}

public class DifferencesDisplayItem
{
    public Guid RegistrationId { get; set; }
    public decimal Price { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRepaid { get; set; }
    public decimal Difference { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public RegistrationState State { get; set; }
    public DateTimeOffset? PaymentDueMailSent { get; set; }
    public DateTimeOffset? TooMuchPaidMailSent { get; set; }
    public string? InternalNotes { get; set; }
}