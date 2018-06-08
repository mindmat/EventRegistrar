using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Users;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Events.UsersInEvents
{
    public static class UsersOfEventQuery
    {
        [FunctionName("UsersOfEventQuery")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{eventAcronym}/users")]
            HttpRequestMessage req,
            TraceWriter log,
            string eventAcronym)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var eventId = await dbContext.Events.GetEventId(eventAcronym);
                await dbContext.Users.AssertUserIsInEventRole(eventId, UserInEventRole.Admin, log);

                var events = dbContext.UsersInEvents
                                      .Where(uie => uie.EventId == eventId)
                                      .Select(uie => new UserInEventDisplayItem
                                      {
                                          EventName = uie.Event.Name,
                                          EventAcronym = uie.Event.Acronym,
                                          EventState = uie.Event.State,
                                          Role = uie.Role,
                                          UserName = uie.User.Name,
                                          UserIdentifier = uie.User.IdentityProviderUserIdentifier
                                      })
                                      .ToListAsync();
                return req.CreateResponse(HttpStatusCode.OK, events);
            }
        }
    }
}