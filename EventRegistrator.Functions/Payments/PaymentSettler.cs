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
                                                       .Include(pmt => pmt.Assignments)
                                                       .ToListAsync();
                if (!unsettledPayments.Any())
                {
                    // nothing to process
                    return;
                }

                var unsettledRegistrations = await dbContext.Registrations
                                                            .Where(reg => reg.RegistrationForm.EventId == eventId && !reg.IsPaid)
                                                            .Include(pmt => pmt.Payments)
                                                            .ToListAsync();

                unsettledPayments.ForEach(payment => TryMatchToRegistration(payment, unsettledRegistrations, dbContext, log));

                await dbContext.SaveChangesAsync();
            }
        }

        private static void TryMatchToRegistration(ReceivedPayment payment, IReadOnlyList<Registration> unsettledRegistrations, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var mails = EmailExtractor.TryExtractEmailFromInfo(payment.Info).ToList();
            payment.RecognizedEmail = string.Join(";", mails);
            if (payment.RecognizedEmail != null)
            {
                var registrations = mails.SelectMany(mail => unsettledRegistrations.Where(reg => reg.RespondentEmail == mail))
                                         .ToList();
                if (!registrations.Any())
                {
                    return;
                }

                var unassignedAmount = payment.Amount - payment.Assignments.Sum(pas => pas.Amount);
                foreach (var registration in registrations.OrderBy(reg => Math.Abs(unassignedAmount - (reg.Price ?? 0m) + reg.Payments.Sum(pmt => pmt.Amount))))
                {
                    var unpaidAmount = (registration.Price ?? 0m) - registration.Payments.Sum(pmt => pmt.Amount);

                    log.Info($"matched {registration.Id}, payment {payment.Id}, unpaid {unpaidAmount}, unassigned {unassignedAmount}");

                    var assignedAmount = Math.Min(unpaidAmount, unassignedAmount);
                    if (assignedAmount > 0m)
                    {
                        dbContext.PaymentAssignments.Add(new PaymentAssignment
                        {
                            Id = Guid.NewGuid(),
                            RegistrationId = registration.Id,
                            ReceivedPaymentId = payment.Id,
                            Amount = assignedAmount
                        });
                        if (assignedAmount == unpaidAmount)
                        {
                            registration.IsPaid = true;
                        }
                        unassignedAmount -= assignedAmount;
                    }
                }
                if (unassignedAmount == 0m)
                {
                    payment.Settled = true;
                }
            }
        }
    }
}