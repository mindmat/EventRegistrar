using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files.Camt;
using EventRegistrar.Backend.Registrations.Cancel;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Refunds
{
    public class RefundsQuery : IRequest<IEnumerable<RefundDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class RefundsQueryHandler : IRequestHandler<RefundsQuery, IEnumerable<RefundDisplayItem>>
    {
        private readonly IQueryable<RegistrationCancellation> _cancellations;

        public RefundsQueryHandler(IQueryable<RegistrationCancellation> cancellations)
        {
            _cancellations = cancellations;
        }

        public async Task<IEnumerable<RefundDisplayItem>> Handle(RefundsQuery query, CancellationToken cancellationToken)
        {
            var refunds = await _cancellations
                .Where(cnc => cnc.Registration.EventId == query.EventId
                           && cnc.Refund > 0m)
                .Select(cnc => new RefundDisplayItem
                {
                    RegistrationId = cnc.RegistrationId,
                    FirstName = cnc.Registration.RespondentFirstName,
                    LastName = cnc.Registration.RespondentLastName,
                    Price = cnc.Registration.Price - cnc.Registration.IndividualReductions.Sum(red => red.Amount),
                    Paid = cnc.Registration.Payments.Sum(ass => ass.Amount),
                    Payments = cnc.Registration.Payments.Where(pmt => pmt.Payment.CreditDebitType == CreditDebit.CRDT)
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
                    RefundPercentage = cnc.RefundPercentage,
                    Refund = cnc.Refund,
                    CancellationDate = cnc.Received ?? cnc.Created,
                    CancellationReason = cnc.Reason
                })
                .OrderByDescending(rpy => rpy.CancellationDate)
                .ToListAsync(cancellationToken);
            return refunds;
        }
    }

    public class RefundDisplayItem
    {
        public Guid RegistrationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal? Price { get; set; }
        public decimal Paid { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal Refund { get; set; }
        public DateTime CancellationDate { get; set; }
        public string CancellationReason { get; set; }
        public IEnumerable<PaymentDisplayItem> Payments { get; set; }
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