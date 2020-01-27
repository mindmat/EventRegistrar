using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Refunds
{
    public class PayoutQuery : IRequest<IEnumerable<PayoutDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class PayoutQueryHandler : IRequestHandler<PayoutQuery, IEnumerable<PayoutDisplayItem>>
    {
        private readonly IQueryable<PayoutRequest> _payoutRequests;

        public PayoutQueryHandler(IQueryable<PayoutRequest> payoutRequests)
        {
            _payoutRequests = payoutRequests;
        }

        public async Task<IEnumerable<PayoutDisplayItem>> Handle(PayoutQuery query, CancellationToken cancellationToken)
        {
            var payouts = await _payoutRequests
                                .Where(por => por.Registration.EventId == query.EventId)
                                .Select(por => new PayoutDisplayItem
                                {
                                    RegistrationId = por.RegistrationId,
                                    Amount = por.Amount,
                                    FirstName = por.Registration.RespondentFirstName,
                                    LastName = por.Registration.RespondentLastName,
                                    Price = (por.Registration.Price ?? 0m),
                                    Paid = por.Registration.Payments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                    Payments = por.Registration.Payments
                                                  .Where(pmt => pmt.Payment.CreditDebitType == CreditDebit.CRDT)
                                                  .Select(pmt => new PaymentDisplayItem
                                                  {
                                                      Assigned = pmt.Amount,
                                                      PaymentAmount = pmt.Payment.Amount,
                                                      PaymentBookingDate = pmt.Payment.BookingDate,
                                                      PaymentDebitorIban = pmt.Payment.DebitorIban,
                                                      PaymentDebitorName = pmt.Payment.DebitorName,
                                                      PaymentMessage = pmt.Payment.Message,
                                                      PaymentInfo = pmt.Payment.Info
                                                  }),
                                    Reason = por.Reason,
                                    StateText = por.State.ToString(),
                                    State = por.State,
                                    Created = por.Created
                                })
                                .OrderByDescending(rpy => rpy.Created)
                                .ToListAsync(cancellationToken);
            return payouts;
        }
    }

    public class PayoutDisplayItem
    {
        public Guid RegistrationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Price { get; set; }
        public decimal Paid { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset Created { get; set; }
        public IEnumerable<PaymentDisplayItem> Payments { get; set; }
        public decimal Amount { get; set; }
        public string StateText { get; set; }
        public PayoutState State { get; set; }
    }

    public class PaymentDisplayItem
    {
        public decimal Assigned { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentBookingDate { get; set; }
        public string PaymentDebitorIban { get; set; }
        public string PaymentDebitorName { get; set; }
        public string PaymentMessage { get; set; }
        public string PaymentInfo { get; set; }
    }
}