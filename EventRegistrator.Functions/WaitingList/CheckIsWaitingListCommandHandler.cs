using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.WaitingList
{
    public static class CheckIsWaitingListCommandHandler
    {
        public const string CheckIsWaitingListCommandsQueueName = "CheckIsWaitingListCommands";

        [FunctionName("CheckIsWaitingListCommandHandler")]
        public static async Task Run([ServiceBusTrigger("CheckIsWaitingListCommands", AccessRights.Listen, Connection = "ServiceBusEndpoint")]
                CheckIsWaitingListCommand command, TraceWriter log)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var registration = await dbContext.Registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId);
                registration.IsWaitingList = await dbContext.Seats
                                                            .AnyAsync(seat => (seat.RegistrableId == command.RegistrationId || 
                                                                               seat.RegistrationId_Follower == command.RegistrationId)
                                                                           && seat.IsWaitingList);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
