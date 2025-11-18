using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments;

public class PaymentOverviewQuery : IRequest<PaymentOverview>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PaymentOverviewQueryHandler(IQueryable<PaymentsFile> paymentFiles,
                                         IQueryable<Registration> registrations,
                                         IDateTimeProvider dateTimeProvider)
    : IRequestHandler<PaymentOverviewQuery, PaymentOverview>
{
    private const int BalanceHistoryMonthsBack = 3;

    public async Task<PaymentOverview> Handle(PaymentOverviewQuery query, CancellationToken cancellationToken)
    {
        var balances = await paymentFiles.Where(pmf => pmf.EventId == query.EventId
                                                    && pmf.BookingsTo >= dateTimeProvider.Now.AddMonths(-BalanceHistoryMonthsBack))
                                         .OrderBy(pmf => pmf.BookingsTo)
                                         .Select(pmf => new
                                                        {
                                                            pmf.AccountIban,
                                                            pmf.Balance,
                                                            pmf.Currency,
                                                            Date = pmf.BookingsTo!.Value
                                                        })
                                         .ToListAsync(cancellationToken);
        var latestBalance = balances.FirstOrDefault();

        var activeRegistrations = await registrations.Where(reg => reg.EventId == query.EventId
                                                                && reg.IsOnWaitingList != true
                                                                && reg.State != RegistrationState.Cancelled)
                                                     .Select(reg => new
                                                                    {
                                                                        reg.Id,
                                                                        reg.State,
                                                                        reg.Price_AdmittedAndReduced,
                                                                        Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.OutgoingPayment == null
                                                                                                                                ? asn.Amount
                                                                                                                                : -asn.Amount)
                                                                    })
                                                     .ToListAsync(cancellationToken);

        return new PaymentOverview
               {
                   Balance = latestBalance == null
                                 ? null
                                 : new BalanceDto
                                   {
                                       Balance = latestBalance.Balance,
                                       Currency = latestBalance.Currency,
                                       AccountIban = latestBalance.AccountIban,
                                       Date = latestBalance.Date
                                   },
                   PaidAmount = activeRegistrations.Sum(reg => reg.Paid ?? 0m),
                   PaidRegistrationsCount = activeRegistrations.Count(reg => reg.State == RegistrationState.Paid),
                   OutstandingAmount = activeRegistrations.Where(reg => reg.State == RegistrationState.Received)
                                                          .Sum(reg => reg.Price_AdmittedAndReduced - (reg.Paid ?? 0m)),
                   BalanceHistory = balances.Select(blc => new BalanceDto
                                                           {
                                                               Balance = blc.Balance,
                                                               Currency = blc.Currency,
                                                               AccountIban = blc.AccountIban,
                                                               Date = blc.Date
                                                           }),
                   NotFullyPaidRegistrations = activeRegistrations.Count(reg => reg.State == RegistrationState.Received)
               };
    }
}

public class PaymentOverview
{
    public BalanceDto? Balance { get; set; }
    public int NotFullyPaidRegistrations { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int PaidRegistrationsCount { get; set; }
    public decimal PaidAmount { get; set; }
    public IEnumerable<OpenSpotsPotential> PotentialOfOpenSpots { get; set; } = null!;
    public IEnumerable<BalanceDto> BalanceHistory { get; set; } = null!;
}