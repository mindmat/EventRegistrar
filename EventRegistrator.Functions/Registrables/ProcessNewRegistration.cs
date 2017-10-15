using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Seats;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Registrables
{
    public static class ProcessNewRegistration
    {
        [FunctionName("ProcessNewRegistration")]
        public static async Task Run([ServiceBusTrigger("ReceivedRegistrations", AccessRights.Listen, Connection = "ServiceBusEndpoint")]RegistrationRegistered @event, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {@event}");
            log.Info($"id {@event.RegistrationId}");

            using (var context = new EventRegistratorDbContext())
            {
                //var registration = await context.Registrations.Where(reg => reg.Id == @event.RegistrationId).Include(reg => reg.Responses).FirstAsync();
                var responses = await context.Responses.Where(rsp => rsp.RegistrationId == @event.RegistrationId).ToListAsync();
                //log.Info(string.Join(",", responses.Select(rbl => rbl.ResponseString)));
                var questionOptionIds = responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value).ToList();
                //log.Info(string.Join(",", questionOptionIds));
                var registrables = await context.QuestionOptionToRegistrableMappings
                                                .Where(map => questionOptionIds.Contains(map.QuestionOptionId))
                                                .Include(map => map.Registrable)
                                                .Include(map => map.Registrable.Seats)
                                                .ToListAsync();

                foreach (var response in responses.Where(rsp => rsp.QuestionOptionId.HasValue))
                {
                    foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
                    {
                        ReserveSeat(context, registrable.Registrable, response, log);
                    }
                }

                //log.Info(string.Join(",", registrables.Select(rbl => rbl.Id)));
                await context.SaveChangesAsync();
            }
        }

        private static void ReserveSeat(EventRegistratorDbContext context, Registrable registrable, Response response, TraceWriter log)
        {
            if (registrable.MaximumSingleSeats.HasValue)
            {
                log.Info($"Registrable {registrable.Name}, Seat count {registrable.Seats.Count}");
                if (registrable.Seats.Count < registrable.MaximumSingleSeats.Value)
                {
                    context.Seats.Add(new Seat
                    {
                        Id = Guid.NewGuid(),
                        RegistrationId = response.RegistrationId,
                        RegistrableId = registrable.Id
                    });
                }
            }
            else if (registrable.MaximumDoubleSeats.HasValue)
            {
            }
            else
            {
                context.Seats.Add(new Seat
                {
                    Id = Guid.NewGuid(),
                    RegistrationId = response.RegistrationId,
                    RegistrableId = registrable.Id
                });
            }
        }
    }
}