using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrables;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.WaitingList
{
    public static class TryPromoteFromWaitingList
    {
        public const string TryPromoteFromWaitingListQueueName = "TryPromoteFromWaitingList";

        [FunctionName("TryPromoteFromWaitingList")]
        public static async Task Run([ServiceBusTrigger(TryPromoteFromWaitingListQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]TryPromoteFromWaitingListCommand command, TraceWriter log)
        {
            log.Info($"TryPromoteFromWaitingList triggered: {command.EventId}, {command.RegistrableId}");

            using (var context = new EventRegistratorDbContext())
            {
                var registrablesToCheck = await context.Registrables.Where(rbl => rbl.EventId == command.EventId &&
                                                                                  (!command.RegistrableId.HasValue || rbl.Id == command.RegistrableId.Value))
                                                                    .Include(rbl => rbl.Seats)
                                                                    .ToListAsync();
                registrablesToCheck.ForEach(TryPromoteFromRegistrableWaitingList);
            }
        }

        private static void TryPromoteFromRegistrableWaitingList(Registrable registrable)
        {
            throw new System.NotImplementedException();
        }
    }
}