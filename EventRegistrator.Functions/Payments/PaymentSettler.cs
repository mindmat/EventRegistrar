using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public static class PaymentSettler
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
                                                            .Where(reg => reg.RegistrationForm.EventId == eventId && (reg.State == RegistrationState.Received || reg.State == RegistrationState.PaymentOverdue))
                                                            .Include(pmt => pmt.Payments)
                                                            .ToListAsync();

                log.Info($"unsettledRegistrations {unsettledRegistrations.Count}, unsettledPayments {unsettledPayments.Count}");
                var registrationIdsToCheck = new List<Guid>();
                foreach (var payment in unsettledPayments)
                {
                    registrationIdsToCheck.AddRange(TryMatchToRegistration(payment, unsettledRegistrations, dbContext, log));
                }

                await dbContext.SaveChangesAsync();

                foreach (var registrationId in registrationIdsToCheck)
                {
                    await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationId, Withhold = true }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
                }
            }
        }

        private static IEnumerable<Guid> TryMatchToRegistration(ReceivedPayment payment, IReadOnlyList<Registration> unsettledRegistrations, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var registrationIdsToCheck = new List<Guid>();
            // RecognizedEmail may be entered manually
            if (payment.RecognizedEmail == null)
            {
                var mails = EmailExtractor.TryExtractEmailFromInfo(payment.Info).ToList();
                var mailsJoined = string.Join(";", mails);
                if (!string.IsNullOrEmpty(mailsJoined))
                {
                    payment.RecognizedEmail = mailsJoined;
                }
            }
            if (payment.RecognizedEmail != null)
            {
                var registrations = payment.RecognizedEmail
                                           .Split(';')
                                           .SelectMany(mail => unsettledRegistrations.Where(reg => reg.RespondentEmail == mail))
                                           .ToList();
                if (!registrations.Any())
                {
                    return registrationIdsToCheck;
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
                        if (assignedAmount == unpaidAmount &&
                            (registration.State == RegistrationState.Received || registration.State == RegistrationState.PaymentOverdue) &&
                            !dbContext.Seats.Any(seat => (seat.RegistrationId == registration.Id || seat.RegistrationId_Follower ==registration.Id) &&
                                                         seat.IsWaitingList &&
                                                         !seat.IsCancelled))
                        {
                            registration.State = RegistrationState.Paid;
                            registrationIdsToCheck.Add(registration.Id);
                        }
                        unassignedAmount -= assignedAmount;
                    }
                }
                if (unassignedAmount == 0m)
                {
                    payment.Settled = true;
                }
            }
            return registrationIdsToCheck;
        }
    }
}