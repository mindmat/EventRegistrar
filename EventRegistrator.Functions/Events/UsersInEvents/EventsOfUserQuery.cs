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

namespace EventRegistrator.Functions.Events
{
    public static class EventsOfUserQuery
    {
        [FunctionName("EventsOfUserQuery")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me/events")]
            HttpRequestMessage req,
            TraceWriter log)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var user = await dbContext.Users.GetAuthenticatedUser(log);
                var events = dbContext.UsersInEvents
                                      .Where(uie => uie.UserId == user.Id)
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