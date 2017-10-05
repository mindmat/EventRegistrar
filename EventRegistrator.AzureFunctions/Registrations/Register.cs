using EventRegistrator.Functions.GoogleForms;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Registrations
{
    public static class Register
    {
        //[FunctionName("Register")]
        //[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registration/{id}")]
        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string id, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //log.Info($"id: {id}");
            //log.Info($"content: {await req.Content.ReadAsStringAsync()}");

            var registration = await req.Content.ReadAsAsync<Registration>();

            log.Info($"registration: id: {registration.Id}, Mail {registration.Email}, Responses{string.Join("|", registration.Responses.Select(q => $"{q.QuestionId}: {string.Join(",", q.Response)}"))}");

            //log.Info(string.Join(Environment.NewLine, registration.Select(itm => $"{itm.Key} = {itm.Value}")));

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}