using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.Payments
{
    public static class ProcessPaymentFiles
    {
        public const string ProcessPaymentFilesQueueName = "ProcessPaymentFiles";

        [FunctionName("ProcessPaymentFiles")]
        public static async Task Run([ServiceBusTrigger(ProcessPaymentFilesQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]ProcessPaymentFilesCommand command, TraceWriter log)
        {
            log.Info($"Processing payment files for Event {command.EventId}");

            await PaymentSettler.Settle(command.EventId, log);
        }
    }
}