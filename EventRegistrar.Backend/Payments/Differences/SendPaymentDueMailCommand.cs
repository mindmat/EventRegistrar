using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Differences
{
    public class SendPaymentDueMailCommand : IRequest, IEventBoundRequest
    {
        public Guid RegistrationId { get; set; }
        public Guid EventId { get; set; }
    }

    public class SendPaymentDueMailCommandHandler : IRequestHandler<SendPaymentDueMailCommand>
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IQueryable<Registration> _registrations;

        public SendPaymentDueMailCommandHandler(ServiceBusClient serviceBusClient,
                                                IQueryable<Registration> registrations)
        {
            _serviceBusClient = serviceBusClient;
            _registrations = registrations;
        }

        public async Task<Unit> Handle(SendPaymentDueMailCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.Payments)
                                                   .FirstAsync(cancellationToken);
            var data = new PaymentDueMailData
            {
                Price = registration.Price ?? 0m,
                AmountPaid = registration.Payments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount)
            };
            if (data.Price <= data.AmountPaid)
            {
                throw new Exception("No money owed");
            }

            var sendMailCommand = new ComposeAndSendMailCommand
            {
                MailType = MailType.MoneyOwed,
                RegistrationId = command.RegistrationId,
                Data = data
            };
            _serviceBusClient.SendMessage(sendMailCommand);

            return Unit.Value;
        }
    }

    public class PaymentDueMailData
    {
        public decimal Price { get; set; }
        public decimal AmountPaid { get; set; }
    }
}