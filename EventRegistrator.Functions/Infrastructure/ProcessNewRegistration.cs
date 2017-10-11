using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.Infrastructure
{
    public static class ProcessNewRegistration
    {
        [FunctionName("ProcessNewRegistration")]
        public static void Run([ServiceBusTrigger("ReceivedRegistrations", AccessRights.Manage, Connection = "ProcessNewRegistration")]RegistrationReceived @event, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {@event}");
            log.Info("$id {@event.RegistrationId}");
        }
    }
}