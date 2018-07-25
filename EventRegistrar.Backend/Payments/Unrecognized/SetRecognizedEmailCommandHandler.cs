using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Payments.Unrecognized
{
    public class SetRecognizedEmailCommandHandler : IRequestHandler<SetRecognizedEmailCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly ILogger _logger;
        private readonly IRepository<ReceivedPayment> _payments;

        public SetRecognizedEmailCommandHandler(IRepository<ReceivedPayment> payments,
                                                ILogger logger,
                                                IEventAcronymResolver acronymResolver)
        {
            _payments = payments;
            _logger = logger;
            _acronymResolver = acronymResolver;
        }

        public async Task<Unit> Handle(SetRecognizedEmailCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);
            var payment = await _payments.Where(rpy => rpy.PaymentFile.EventId == eventId
                                                    && rpy.Id == command.PaymentId)
                                         .Include(rpy => rpy.PaymentFile)
                                         .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("No payment found with id {0}", command.PaymentId);
                return Unit.Value;
            }
            if (payment.RecognizedEmail != null)
            {
                _logger.LogInformation("Payment {0} already is recognized: {1}", command.PaymentId, payment.RecognizedEmail);
                return Unit.Value;
            }

            payment.RecognizedEmail = command.Email;

            // ToDo
            //await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand { EventId = eventId.Value }, ProcessPaymentFiles.ProcessPaymentFilesQueueName);
            return Unit.Value;
        }
    }
}