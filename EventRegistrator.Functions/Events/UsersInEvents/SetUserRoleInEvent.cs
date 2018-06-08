using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Users;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Events.UsersInEvents
{
    public static class SetUserRoleInEvent
    {
        [FunctionName("SetUserRoleInEvent")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "{eventAcronym}/users/{userIdString:guid}")]
            HttpRequestMessage req,
            TraceWriter log,
            string eventAcronym,
            string userIdString)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var eventId = await dbContext.Events.GetEventId(eventAcronym);
                await dbContext.Users.AssertUserIsInEventRole(eventId, UserInEventRole.Admin, log);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}