using EventRegistrator.Functions.GoogleForms;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
            form.Identifier = id;
            log.Info($"form: id {form.Identifier}, Title {form.Title}, Questions {string.Join("|", form.Questions.Select(q => $"{q.Id}({q.Index}): {q.Title} ({q.Type})"))}");

            var entity = Convert(form);
            using (var context = new EventRegistratorDbContext())
            {
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private static RegistrationForm Convert(FormDescription input)
        {
            return new RegistrationForm
            {
                Identifier = input.Identifier,
                Title = input.Title,
                Questions = input.Questions.Select(qst => new Question
                {
                    ExternalId = qst.Id,
                    Index = qst.Index,
                    Title = qst.Title,
                    Type = (QuestionType)qst.Type
                })
            };
        }
    }
}