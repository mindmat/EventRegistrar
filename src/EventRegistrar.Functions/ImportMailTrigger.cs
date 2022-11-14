using System.Threading.Tasks;

using Microsoft.Azure.Functions.Worker;

namespace EventRegistrar.Functions;

public static class ImportMailTrigger
{
    [Function(nameof(ImportMailTrigger))]
    public static async Task Run([TimerTrigger("0 0 */2 * * *")] TimerInfo _)
    {
        await CommandQueue.SendCommand("EventRegistrar.Backend.Mailing.Import.ImportMailsFromImapForAllActiveEventsCommand");
    }
}