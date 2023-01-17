using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments;

public class PaymentOverviewQuery : IRequest<PaymentOverview>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PaymentOverviewQueryHandler : IRequestHandler<PaymentOverviewQuery, PaymentOverview>
{
    private readonly IQueryable<PaymentsFile> _paymentFiles;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IQueryable<Registration> _registrations;
    private const int BalanceHistoryMonthsBack = 3;

    public PaymentOverviewQueryHandler(IQueryable<PaymentsFile> paymentFiles,
                                       IQueryable<Registration> registrations,
                                       IQueryable<Registrable> registrables,
                                       IDateTimeProvider dateTimeProvider)
    {
        _paymentFiles = paymentFiles;
        _registrations = registrations;
        _registrables = registrables;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PaymentOverview> Handle(PaymentOverviewQuery query, CancellationToken cancellationToken)
    {
        var balances = await _paymentFiles.Where(pmf => pmf.EventId == query.EventId
                                                     && pmf.BookingsTo >= _dateTimeProvider.Now.AddMonths(-BalanceHistoryMonthsBack))
                                          .OrderByDescending(pmf => pmf.BookingsTo ?? DateTime.MinValue)
                                          .Select(pmf => new
                                                         {
                                                             pmf.AccountIban,
                                                             pmf.Balance,
                                                             pmf.Currency,
                                                             Date = pmf.BookingsTo
                                                         })
                                          .ToListAsync(cancellationToken);
        var latestBalance = balances.FirstOrDefault();

        var activeRegistrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                 && reg.IsOnWaitingList != true
                                                                 && reg.State != RegistrationState.Cancelled)
                                                      .Select(reg => new
                                                                     {
                                                                         reg.Id,
                                                                         reg.State,
                                                                         reg.Price_AdmittedAndReduced,
                                                                         Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                                                 ? asn.Amount
                                                                                                                                 : -asn.Amount)
                                                                     })
                                                      .ToListAsync(cancellationToken);

        var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                         && (rbl.MaximumDoubleSeats != null || rbl.MaximumSingleSeats != null)
                                                         && rbl.Price != null)
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .Select(rbl => new
                                                             {
                                                                 RegistrableId = rbl.Id,
                                                                 rbl.DisplayName,
                                                                 Price = rbl.Price!.Value,
                                                                 SpotsAvailable = rbl.MaximumSingleSeats ?? rbl.MaximumDoubleSeats!.Value * 2,
                                                                 LeaderCount = rbl.Spots!
                                                                                  .Where(spot => !spot.IsCancelled && !spot.IsWaitingList)
                                                                                  .Count(spot => spot.RegistrationId != null),
                                                                 FollowerCount = rbl.Spots!
                                                                                    .Where(spot => !spot.IsCancelled && !spot.IsWaitingList)
                                                                                    .Count(spot => spot.RegistrationId_Follower != null)
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
                                       Date = latestBalance.Date?.Date
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
                                                               Date = blc.Date?.Date
                                                           }),
                   NotFullyPaidRegistrations = activeRegistrations.Count(reg => reg.State == RegistrationState.Received),
                   PotentialOfOpenSpots = registrables.Select(rbl => new OpenSpotsPotential
                                                                     {
                                                                         RegistrableId = rbl.RegistrableId,
                                                                         Name = rbl.DisplayName,
                                                                         SpotsAvailable = Math.Max(0, rbl.SpotsAvailable - rbl.LeaderCount - rbl.FollowerCount),
                                                                         PotentialIncome = Math.Max(0, rbl.SpotsAvailable - rbl.LeaderCount - rbl.FollowerCount)
                                                                                         * rbl.Price
                                                                     })
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