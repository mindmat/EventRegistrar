using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentOverviewQueryHandler : IRequestHandler<PaymentOverviewQuery, PaymentOverview>
    {
        private readonly IQueryable<PaymentFile> _paymentFiles;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IQueryable<Registration> _registrations;

        public PaymentOverviewQueryHandler(IQueryable<PaymentFile> paymentFiles,
                                           IQueryable<Registration> registrations,
                                           IQueryable<Registrable> registrables)
        {
            _paymentFiles = paymentFiles;
            _registrations = registrations;
            _registrables = registrables;
        }

        public async Task<PaymentOverview> Handle(PaymentOverviewQuery query, CancellationToken cancellationToken)
        {
            var balance = await _paymentFiles.Where(pmf => pmf.EventId == query.EventId)
                                             .OrderByDescending(pmf => pmf.BookingsTo ?? DateTime.MinValue)
                                             .Select(pmf => new
                                             {
                                                 pmf.AccountIban,
                                                 pmf.Balance,
                                                 pmf.Currency,
                                                 Date = pmf.BookingsTo
                                             })
                                             .FirstOrDefaultAsync(cancellationToken);

            var activeRegistrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                     && reg.IsWaitingList != true
                                                                     && reg.State != RegistrationState.Cancelled)
                                                          .Select(reg => new
                                                          {
                                                              reg.Id,
                                                              reg.State,
                                                              reg.Price,
                                                              Paid = (decimal?)reg.Payments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
                                                          })
                                                          .ToListAsync(cancellationToken);

            var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                             && (rbl.MaximumDoubleSeats.HasValue || rbl.MaximumSingleSeats.HasValue)
                                                             && rbl.Price.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Select(rbl => new
                                                  {
                                                      RegistrableId = rbl.Id,
                                                      rbl.Name,
                                                      Price = rbl.Price.Value,
                                                      SpotsAvailable = rbl.MaximumSingleSeats ?? rbl.MaximumDoubleSeats.Value * 2,
                                                      LeaderCount = rbl.Seats.Where(seat => !seat.IsCancelled && !seat.IsWaitingList).Count(seat => seat.RegistrationId != null),
                                                      FollowerCount = rbl.Seats.Where(seat => !seat.IsCancelled && !seat.IsWaitingList).Count(seat => seat.RegistrationId_Follower != null),
                                                  })
                                                  .ToListAsync(cancellationToken);

            return new PaymentOverview
            {
                Balance = balance == null ? null : new BalanceDto
                {
                    Balance = balance.Balance,
                    Currency = balance.Currency,
                    AccountIban = balance.AccountIban,
                    Date = balance.Date?.Date
                },
                ReceivedMoney = activeRegistrations.Sum(reg => reg.Paid ?? 0m),
                PaidRegistrations = activeRegistrations.Count(reg => reg.State == RegistrationState.Paid),
                OutstandingAmount = activeRegistrations.Where(reg => reg.State == RegistrationState.Received)
                                                       .Sum(reg => (reg.Price ?? 0m) - (reg.Paid ?? 0m)),
                NotFullyPaidRegistrations = activeRegistrations.Count(reg => reg.State == RegistrationState.Received),
                PotentialOfOpenSpots = registrables.Select(rbl => new OpenSpotsPotential
                {
                    RegistrableId = rbl.RegistrableId,
                    Name = rbl.Name,
                    SpotsAvailable = Math.Max(0, rbl.SpotsAvailable - rbl.LeaderCount - rbl.FollowerCount),
                    PotentialIncome = Math.Max(0, rbl.SpotsAvailable - rbl.LeaderCount - rbl.FollowerCount) * rbl.Price
                })
            };
        }
    }
}