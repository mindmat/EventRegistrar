using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Registrations
{
    public static class Register
    {
        private const string ConnectionString = "Endpoint=sb://eventregistrator.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=DOPXPS/i/CJy7O7UxqdhGDAxacjZJhZMwfj+k311qCI=";

        [FunctionName("Register")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrationform/{formId}/registration/{id}")]HttpRequestMessage req,
            string formId,
            string id,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //log.Info($"id: {id}");
            //log.Info($"content: {await req.Content.ReadAsStringAsync()}");

            var googleRegistration = await req.Content.ReadAsAsync<GoogleForms.Registration>();

            //log.Info($"registration: id: {registration.Id}, Mail {registration.Email}, Responses{string.Join("|", registration.Responses.Select(q => $"{q.QuestionId}: {string.Join(",", q.Response)}"))}");

            //log.Info(string.Join(Environment.NewLine, registration.Select(itm => $"{itm.Key} = {itm.Value}")));

            RegistrationReceived registeredEvent;
            using (var context = new EventRegistratorDbContext())
            {
                var form = await context.RegistrationForms
                                        .Where(frm => frm.ExternalIdentifier == formId)
                                        .Include(frm => frm.Questions)
                                        .FirstOrDefaultAsync();
                if (form == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent($"No form found with id '{formId}'")
                    };
                }

                // check form state
                if (form.State == State.RegistrationClosed)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Registration is closed")
                    };
                }
                if (!form.EventId.HasValue)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Registration is not yet assigned to an event")
                    };
                }

                var registration = await context.Registrations.FirstOrDefaultAsync(reg => reg.ExternalIdentifier == id);
                if (registration == null)
                {
                    registration = new Registration
                    {
                        Id = Guid.NewGuid(),
                        ExternalIdentifier = id,
                        RegistrationFormId = form.Id,
                        ReceivedAt = DateTime.UtcNow,
                        RespondentEmail = googleRegistration.Email
                    };
                    foreach (var response in googleRegistration.Responses)
                    {
                        var question = form.Questions?.FirstOrDefault(qst => qst.ExternalId == response.QuestionExternalId);
                        context.Responses.Add(new Response
                        {
                            Id = Guid.NewGuid(),
                            ResponseString = response.Response,
                            QuestionId = question?.Id
                        });
                    }
                    context.Registrations.Add(registration);
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent($"Registration with id '{id}' already exists")
                    };
                }

                await context.SaveChangesAsync();
                registeredEvent = new RegistrationReceived
                {
                    RegistrationId = registration.Id,
                    Registration = registration
                };
            }

            var client = QueueClient.CreateFromConnectionString(ConnectionString, "ReceivedRegistrations");
            var message = new BrokeredMessage(registeredEvent);
            await client.SendAsync(message);

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}