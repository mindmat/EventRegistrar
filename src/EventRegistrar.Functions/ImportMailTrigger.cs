using System;
using System.Threading.Tasks;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions;

public static class ImportMailTrigger
{
    [Function(nameof(ImportMailTrigger))]
    public static async Task Run([TimerTrigger("0 0 */2 * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        var command = new
                      {
                          CommandType = "EventRegistrar.Backend.Mailing.Import.ImportMailsFromImapForAllActiveEventsCommand",
                          CommandSerialized = "{}"
                      };
        await CommandQueue.SendCommand(command);
    }
}