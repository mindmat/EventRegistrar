using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public static class CamtProcessor
    {
        public static async Task Process(Stream camtStream, TraceWriter log)
        {
            var xml = XDocument.Load(camtStream);
            var content = xml.ToString();

            var camt = CamtParser.Parse(xml);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var existingFile = dbContext.PaymentFiles.FirstOrDefault(fil => fil.FileId == camt.FileId);
                if (existingFile != null)
                {
                    log.Info($"File with Id {camt.FileId} already exists (PaymentFile.Id = {existingFile.Id})");
                    return;
                }

                var @event = dbContext.Events.FirstOrDefault(evt => evt.AccountIban == camt.Account);

                var paymentFile = new PaymentFile
                {
                    Id = Guid.NewGuid(),
                    EventId = @event?.Id,
                    Content = content,
                    FileId = camt.FileId,
                    AccountIban = camt.Account,
                    Balance = camt.Balance,
                    Currency = camt.Currency
                };

                dbContext.PaymentFiles.Add(paymentFile);
                foreach (var camtEntry in camt.Entries.Where(cmt => cmt.Type == CreditDebit.CRDT))
                {
                    dbContext.ReceivedPayments.Add(new ReceivedPayment
                    {
                        Id = Guid.NewGuid(),
                        PaymentFileId = paymentFile.Id,
                        Info = camtEntry.Info,
                        Amount = camtEntry.Amount,
                        BookingDate = camtEntry.BookingDate,
                        Currency = camtEntry.Currency,
                        Reference = camtEntry.Reference,
                    });
                }
                await dbContext.SaveChangesAsync();
                if (@event != null)
                {
                    await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand { EventId = @event.Id }, ProcessPaymentFiles.ProcessPaymentFilesQueueName);
                }
            }
        }
    }
}