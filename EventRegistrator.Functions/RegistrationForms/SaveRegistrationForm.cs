using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.RegistrationForms
{
    public static class SaveRegistrationForm
    {
        [FunctionName("SaveRegistrationForm")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrationform/{id}")] HttpRequestMessage req,
            string id, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //log.Info($"id: {id}");
            //log.Info($"content: {await req.Content.ReadAsStringAsync()}");

            var form = await req.Content.ReadAsAsync<FormDescription>();
            form.Id = id;
            log.Info($"form: id {form.Id}, Title {form.Title}, Questions {string.Join("|", form.Questions.Select(q => $"{q.Id}({q.Index}): {q.Title} ({q.Type})"))}");

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}