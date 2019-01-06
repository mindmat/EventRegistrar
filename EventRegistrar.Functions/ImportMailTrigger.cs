using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions
{
    public static class ImportMailTrigger
    {
        [FunctionName("ImportMailTrigger")]
        public static async Task Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var command = new
            {
                CommandType = "EventRegistrar.Backend.Mailing.Import.ImportMailsFromImapForAllActiveEventsCommand",
                CommandSerialized = "{}"
            };
            await ServiceBusClient.SendCommand(command, "CommandQueue");
        }
    }
}