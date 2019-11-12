using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Files.Camt;
using EventRegistrar.Backend.Payments.Files.Slips;
using ICSharpCode.SharpZipLib.Tar;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Payments.Files
{
    public class SavePaymentFileCommandHandler : IRequestHandler<SavePaymentFileCommand>
    {
        private const string PostfinancePaymentSlipFilenameRegex = @"^camt\.053_._(?<IBAN>CH[0-9]{19})_\d+_\d+_\d+-(?<ID>\d+)\.(?<Extension>[a-z]+)";

        private readonly CamtParser _camtParser;
        private readonly IEventBus _eventBus;
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
                                             IEventBus eventBus)
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
            command.FileStream.Position = 0;
            switch (command.ContentType)
            {
                case "text/xml":
                case "application/xml":
                    await SaveCamt(command.EventId, command.FileStream, cancellationToken);
                    break;

                case "image/jpeg":
                case "image/png":
                    await TrySavePaymentSlipImage(command.EventId, command.FileStream, command.Filename, command.ContentType);
                    break;

                case "application/x-gzip":
                    await SaveCamtWithPaymentSlips(command.EventId, command.FileStream, cancellationToken);
                    break;
            }

            return Unit.Value;
        }

        private static string ConvertExtensionToContentType(string extension)
        {
            switch (extension)
            {
                case ".tiff":
                case ".tif":
                    return "image/tiff";

                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";

                case ".png":
                    return "image/png";

                default:
                    return extension;
            }
        }

        private async Task<IEnumerable<ReceivedPayment>> SaveCamt(Guid eventId, Stream stream, CancellationToken cancellationToken)
        {
            var newPayments = new List<ReceivedPayment>();
            stream.Position = 0;
            var xml = XDocument.Load(stream);

            var camt = _camtParser.Parse(xml);

            var existingFile = await _paymentFiles.FirstOrDefaultAsync(fil => fil.EventId == eventId
                                                                           && fil.FileId == camt.FileId, cancellationToken);
            if (existingFile != null)
            {
                _log.LogInformation($"File with Id {camt.FileId} already exists (PaymentFile.Id = {existingFile.Id})");
                return newPayments;
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
                var newPayment = new ReceivedPayment
                {
                    Id = Guid.NewGuid(),
                    PaymentFileId = paymentFile.Id,
                    Info = camtEntry.Info,
                    Message = camtEntry.Message,
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
                };
                await _payments.InsertOrUpdateEntity(newPayment, cancellationToken);
                newPayments.Add(newPayment);
            }

            if (@event != null)
            {
                //await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand { EventId = @event.Id }, ProcessPaymentFiles.ProcessPaymentFilesQueueName);
            }

            return newPayments;
        }

        private async Task SaveCamtWithPaymentSlips(Guid eventId, MemoryStream stream, CancellationToken cancellationToken)
        {
            using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (var tarStream = new TarInputStream(zipStream))
                {
                    TarEntry entry;
                    var newLines = new List<ReceivedPayment>();

                    while ((entry = tarStream.GetNextEntry()) != null)
                    {
                        var outStream = new MemoryStream();

                        if (entry.Name.EndsWith(".xml"))
                        {
                            tarStream.CopyEntryContents(outStream);
                            outStream.Position = 0;
                            newLines.AddRange(await SaveCamt(eventId, outStream, cancellationToken));
                        }
                        else if (entry.Name.EndsWith(".tiff"))
                        {
                            var fileInfo = new FileInfo(entry.Name);
                            var matches = Regex.Match(entry.Name, PostfinancePaymentSlipFilenameRegex);
                            tarStream.CopyEntryContents(outStream);
                            outStream.Position = 0;
                            var reference = matches.Groups["ID"].Value;
                            var iban = matches.Groups["IBAN"].Value;
                            byte[] binary;
                            string extension;
                            if (fileInfo.Extension == ".tiff" || fileInfo.Extension == ".tif")
                            {
                                // chrome doesn't support tiff, so convert it to png
                                extension = "image/png";
                                var pngStream = new MemoryStream();
                                new Bitmap(outStream).Save(pngStream, ImageFormat.Png);
                                binary = pngStream.ToArray();
                            }
                            else
                            {
                                extension = ConvertExtensionToContentType(fileInfo.Extension);
                                binary = outStream.ToArray();
                            }

                            var paymentSlip = new PaymentSlip
                            {
                                EventId = eventId,
                                ContentType = extension,
                                FileBinary = binary,
                                Filename = entry.Name,
                                Reference = reference
                            };
                            await _paymentSlips.InsertOrUpdateEntity(paymentSlip, cancellationToken);

                            _eventBus.Publish(new PaymentSlipReceived
                            {
                                EventId = eventId,
                                Reference = reference,
                                PaymentSlipId = paymentSlip.Id
                            });
                        }
                    }
                }
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