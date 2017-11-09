using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;

namespace EventRegistrator.Functions.Payments
{
    public class PaymentSettler
    {
        public static async Task Settle(Guid eventId)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var unsettledPayments = await dbContext.ReceivedPayments
                                                       .Where(pmt => pmt.EventId == eventId && !pmt.Settled)
                                                       .ToListAsync();
                if (!unsettledPayments.Any())
                {
                    // nothing to process
                    return;
                }

                var unsettledRegistrations = await dbContext.Registrations
                                                            .Where(reg => reg.RegistrationForm.EventId == eventId && !reg.IsPayed)
                                                            .ToListAsync();

                unsettledPayments.ForEach(payment => TryMatchToRegistration(payment, unsettledRegistrations));
            }
        }

        private static void TryMatchToRegistration(ReceivedPayment payment, IReadOnlyList<Registration> unsettledRegistrations)
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
                }
            }
        }
    }
}