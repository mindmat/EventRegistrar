using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files.Camt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Payments.Files
{
    public class SavePaymentFileCommandHandler : IRequestHandler<SavePaymentFileCommand>
    {
        private readonly CamtParser _camtParser;
        private readonly EventBus _eventBus;
        private readonly IQueryable<Event> _events;
        private readonly ILogger _log;
        private readonly IRepository<PaymentFile> _paymentFiles;
        private readonly IRepository<ReceivedPayment> _payments;
        private readonly IRepository<PaymentSlip> _paymentSlips;

        public SavePaymentFileCommandHandler(IRepository<PaymentFile> paymentFiles,
                                             IRepository<ReceivedPayment> payments,
                                             IRepository<PaymentSlip> paymentSlips,
                                             IQueryable<Event> events,
                                             CamtParser camtParser,
                                             ILogger log,
                                             EventBus eventBus)
        {
            _paymentFiles = paymentFiles;
            _payments = payments;
            _paymentSlips = paymentSlips;
            _events = events;
            _camtParser = camtParser;
            _log = log;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(SavePaymentFileCommand command, CancellationToken cancellationToken)
        {
            switch (command.ContentType)
            {
                case "application/xml":
                    await SaveCamt(command.EventId, command.FileStream, cancellationToken);
                    break;

                case "image/jpeg":
                    await TrySavePaymentSlipImage(command.EventId, command.FileStream, command.Filename, command.ContentType);
                    break;
            }

            return Unit.Value;
        }

        private async Task SaveCamt(Guid eventId, Stream stream, CancellationToken cancellationToken)
        {
            stream.Position = 0;
            var xml = XDocument.Load(stream);

            var camt = _camtParser.Parse(xml);

            var existingFile = await _paymentFiles.FirstOrDefaultAsync(fil => fil.EventId == eventId
                                                                           && fil.FileId == camt.FileId, cancellationToken);
            if (existingFile != null)
            {
                _log.LogInformation($"File with Id {camt.FileId} already exists (PaymentFile.Id = {existingFile.Id})");
                return;
            }

            var @event = await _events.FirstOrDefaultAsync(evt => evt.AccountIban == camt.Account, cancellationToken);

            var paymentFile = new PaymentFile
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Content = xml.ToString(),
                FileId = camt.FileId,
                AccountIban = camt.Account,
                Balance = camt.Balance,
                Currency = camt.Currency,
                BookingsFrom = camt.BookingsFrom,
                BookingsTo = camt.BookingsTo
            };

            await _paymentFiles.InsertOrUpdateEntity(paymentFile, cancellationToken);
            foreach (var camtEntry in camt.Entries)
            {
                await _payments.InsertOrUpdateEntity(new ReceivedPayment
                {
                    Id = Guid.NewGuid(),
                    PaymentFileId = paymentFile.Id,
                    Info = camtEntry.Info,
                    Amount = camtEntry.Amount,
                    BookingDate = camtEntry.BookingDate,
                    Currency = camtEntry.Currency,
                    Reference = camtEntry.Reference,
                    CreditDebitType = camtEntry.Type,
                    Charges = camtEntry.Charges,
                    DebitorName = camtEntry.DebitorName,
                    DebitorIban = camtEntry.DebitorIban,
                    InstructionIdentification = camtEntry.InstructionIdentification,
                    RawXml = camtEntry.Xml
                }, cancellationToken);
            }

            if (@event != null)
            {
                //await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand { EventId = @event.Id }, ProcessPaymentFiles.ProcessPaymentFilesQueueName);
            }
        }

        private async Task TrySavePaymentSlipImage(Guid eventId,
                                                   MemoryStream fileStream,
                                                   string filename,
                                                   string contentType)
        {
            var reference = filename.Split('.').First();
            var paymentSlip = new PaymentSlip
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                FileBinary = fileStream.ToArray(),
                Filename = filename,
                Reference = reference,
                ContentType = contentType
            };
            await _paymentSlips.InsertOrUpdateEntity(paymentSlip);
            _eventBus.Publish(new PaymentSlipReceived
            {
                EventId = eventId,
                Reference = reference,
                PaymentSlipId = paymentSlip.Id
            });
        }
    }
}