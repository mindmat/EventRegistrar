using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Registrables
{
    public static class ProcessNewRegistration
    {
        [FunctionName("ProcessNewRegistration")]
        public static async Task Run([ServiceBusTrigger("ReceivedRegistrations", AccessRights.Listen, Connection = "ServiceBusEndpoint")]RegistrationReceived @event, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {@event}");
            log.Info($"id {@event.RegistrationId}");

            using (var context = new EventRegistratorDbContext())
            {
                //var registration = await context.Registrations.Where(reg => reg.Id == @event.RegistrationId).Include(reg => reg.Responses).FirstAsync();
                var responses = await context.Responses.Where(rsp => rsp.RegistrationId == @event.RegistrationId).ToListAsync();
            }
        }
    }
}