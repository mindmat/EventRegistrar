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
    public class SendTooMuchPaidMailCommand : IRequest, IEventBoundRequest
    {
        public Guid RegistrationId { get; set; }
        public Guid EventId { get; set; }
    }

    public class SendTooMuchPaidMailCommandHandler : IRequestHandler<SendTooMuchPaidMailCommand>
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IQueryable<Registration> _registrations;

        public SendTooMuchPaidMailCommandHandler(ServiceBusClient serviceBusClient,
                                                 IQueryable<Registration> registrations)
        {
            _serviceBusClient = serviceBusClient;
            _registrations = registrations;
        }

        public async Task<Unit> Handle(SendTooMuchPaidMailCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.Payments)
                                                   .FirstAsync(cancellationToken);
            var data = new TooMuchPaidMailData
            {
                Price = registration.Price ?? 0m,
                AmountPaid = registration.Payments.Sum(pmt => pmt.Amount)
            };
            if (data.Price >= data.AmountPaid)
            {
                throw new Exception("Not too much paid");
            }

            var sendMailCommand = new ComposeAndSendMailCommand
            {
                MailType = MailType.TooMuchPaid,
                RegistrationId = command.RegistrationId,
                Data = data
            };
            _serviceBusClient.SendMessage(sendMailCommand);

            return Unit.Value;
        }
    }

    public class TooMuchPaidMailData
    {
        public decimal Price { get; set; }
        public decimal AmountPaid { get; set; }
    }
}