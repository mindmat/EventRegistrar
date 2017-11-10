using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public class PaymentSettler
    {
        public static async Task Settle(Guid eventId, TraceWriter log)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var unsettledPayments = await dbContext.ReceivedPayments
                                                       .Where(pmt => pmt.PaymentFile.EventId == eventId && !pmt.Settled)
                                                       .ToListAsync();
                if (!unsettledPayments.Any())
                {
                    // nothing to process
                    return;
                }

                var unsettledRegistrations = await dbContext.Registrations
                                                            .Where(reg => reg.RegistrationForm.EventId == eventId && !reg.IsPayed)
                                                            .ToListAsync();

                unsettledPayments.ForEach(payment => TryMatchToRegistration(payment, unsettledRegistrations, log));
            }
        }

        private static void TryMatchToRegistration(ReceivedPayment payment, IReadOnlyList<Registration> unsettledRegistrations, TraceWriter log)
        {
            var mails = EmailExtractor.TryExtractEmailFromInfo(payment.Info);
            payment.RecognizedEmail = string.Join(";", mails);
            if (payment.RecognizedEmail != null)
            {
                var registrations = mails.Select(mail => unsettledRegistrations.FirstOrDefault(reg => reg.RespondentEmail == mail))
                                         .Where(reg => reg != null)
                                         .ToList();
                if (!registrations.Any())
                {
                    return;
                }
                var price = registrations.Sum(reg => reg.Price);
                foreach (var registration in registrations)
                {
                    log.Info($"matched {registration.Id}, payment {payment.Id}, amount {payment.Amount}");
                }
            }
        }
    }
}