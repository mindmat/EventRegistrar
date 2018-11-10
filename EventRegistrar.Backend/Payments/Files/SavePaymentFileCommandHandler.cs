using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files.Camt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Payments.Files
{
    public class SavePaymentFileCommandHandler : IRequestHandler<SavePaymentFileCommand>
    {
        private readonly CamtParser _camtParser;
        private readonly IQueryable<Event> _events;
        private readonly ILogger _log;
        private readonly IRepository<PaymentFile> _paymentFiles;
        private readonly IRepository<ReceivedPayment> _payments;

        public SavePaymentFileCommandHandler(IRepository<PaymentFile> paymentFiles,
                                             IRepository<ReceivedPayment> payments,
                                             IQueryable<Event> events,
                                             CamtParser camtParser,
                                             ILogger log)
        {
            _paymentFiles = paymentFiles;
            _payments = payments;
            _events = events;
            _camtParser = camtParser;
            _log = log;
        }

        public async Task<Unit> Handle(SavePaymentFileCommand command, CancellationToken cancellationToken)
        {
            command.FileStream.Position = 0;
            var xml = XDocument.Load(command.FileStream);
            var content = xml.ToString();

            var camt = _camtParser.Parse(xml);

            var existingFile = await _paymentFiles.FirstOrDefaultAsync(fil => fil.EventId == command.EventId && fil.FileId == camt.FileId, cancellationToken);
            if (existingFile != null)
            {
                _log.LogInformation($"File with Id {camt.FileId} already exists (PaymentFile.Id = {existingFile.Id})");
                return Unit.Value;
            }

            var @event = await _events.FirstOrDefaultAsync(evt => evt.AccountIban == camt.Account, cancellationToken);

            var paymentFile = new PaymentFile
            {
                Id = Guid.NewGuid(),
                EventId = command.EventId,
                Content = content,
                FileId = camt.FileId,
                AccountIban = camt.Account,
                Balance = camt.Balance,
                Currency = camt.Currency,
                BookingsFrom = camt.BookingsFrom,
                BookingsTo = camt.BookingsTo
            };

            await _paymentFiles.InsertOrUpdateEntity(paymentFile, cancellationToken);
            foreach (var camtEntry in camt.Entries)//.Where(cmt => cmt.Type == CreditDebit.CRDT))
            {
                await _payments.InsertOrUpdateEntity(new ReceivedPayment
                {
                    Id = Guid.NewGuid(),
                    PaymentFileId = paymentFile.Id,
                    Info = camtEntry.Info,
                    Amount = camtEntry.Amount,
                    BookingDate = camtEntry.BookingDate,
                    Currency = camtEntry.Currency,
                    Reference = camtEntry.Reference
                }, cancellationToken);
            }
            if (@event != null)
            {
                //await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand { EventId = @event.Id }, ProcessPaymentFiles.ProcessPaymentFilesQueueName);
            }

            return Unit.Value;
        }
    }
}