using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.GoogleForms;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.RegistrationForms;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using QuestionType = EventRegistrator.Functions.RegistrationForms.QuestionType;

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
            var googleRegistration = await req.Content.ReadAsAsync<GoogleForms.Registration>();

            log.Info(await req.Content.ReadAsStringAsync());

            RegistrationReceived registeredEvent;
            using (var context = new EventRegistratorDbContext())
            {
                var form = await context.RegistrationForms
                                        .Where(frm => frm.ExternalIdentifier == formId)
                                        .Include(frm => frm.Questions.Select(qst => qst.QuestionOptions))
                                        .Include(frm => frm.Questions)
                                        .FirstOrDefaultAsync();
                if (form == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent($"No form found with id '{formId}'")
                    };
                }
                log.Info($"Questions: {form.Questions.Count}, Options: {form.Questions.Sum(qst => qst.QuestionOptions.Count)}");

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
                        ExternalTimestamp = googleRegistration.Timestamp,
                        RespondentEmail = googleRegistration.Email
                    };
                    foreach (var response in googleRegistration.Responses)
                    {
                        var responseLookup = LookupResponse(response, form.Questions, log);
                        if (responseLookup.questionOptionId.Any())
                        {
                            foreach (var questionOptionId in responseLookup.questionOptionId)
                            {
                                context.Responses.Add(new Response
                                {
                                    Id = Guid.NewGuid(),
                                    RegistrationId = registration.Id,
                                    ResponseString = string.IsNullOrEmpty(response.Response) ? string.Join(", ", response.Responses) : response.Response,
                                    QuestionId = responseLookup.questionId,
                                    QuestionOptionId = questionOptionId
                                });
                            }
                        }
                        else
                        {
                            context.Responses.Add(new Response
                            {
                                Id = Guid.NewGuid(),
                                RegistrationId = registration.Id,
                                ResponseString = string.IsNullOrEmpty(response.Response) ? string.Join(", ", response.Responses) : response.Response,
                                QuestionId = responseLookup.questionId,
                            });
                        }
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
                registeredEvent = new RegistrationReceived
                {
                    RegistrationId = registration.Id,
                    Registration = registration
                };
                context.DomainEvents.Save(registeredEvent, form.Id);

                await context.SaveChangesAsync();
            }

            var client = QueueClient.CreateFromConnectionString(ConnectionString, "ReceivedRegistrations");
            var message = new BrokeredMessage(registeredEvent);
            await client.SendAsync(message);

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static (Guid? questionId, IEnumerable<Guid> questionOptionId) LookupResponse(ResponseData response, IEnumerable<Question> questions, TraceWriter log)
        {
            var question = questions?.FirstOrDefault(qst => qst.ExternalId == response.QuestionExternalId);
            if (question?.Type == QuestionType.Checkbox && response.Responses.Any())
            {
                log.Info(string.Join(", ", response.Responses));
                var optionIds = question.QuestionOptions?.Where(qop => response.Responses.Any(rsp => rsp == qop.Answer)).Select(qop => qop.Id).ToList();
                log.Info(optionIds?.Count.ToString());
                return (question.Id, optionIds);
            }
            var optionId = question?.QuestionOptions?.Where(qop => qop.Answer == response.Response).FirstOrDefault()?.Id;
            return (question?.Id, optionId.HasValue ? new[] { optionId.Value } : new Guid[] { });
        }
    }
}