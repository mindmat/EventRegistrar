using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.GoogleForms;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Data.Entity;
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

            var googleForm = await req.Content.ReadAsAsync<FormDescription>();
            //googleForm.Identifier = id;
            //log.Info($"form: id {form.Identifier}, Title {form.Title}, Questions {string.Join("|", form.Questions.Select(q => $"{q.Id}({q.Index}): {q.Title} ({q.Type})"))}");

            using (var context = new EventRegistratorDbContext())
            {
                var form = await context.RegistrationForms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == id);
                if (form != null)
                {
                    // update existing form
                    if (form.State != State.Setup)
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent("Registration form can only be changed in state 'setup'")
                        };
                    }

                    form.Title = googleForm.Title;
                }
                else
                {
                    // new form
                    form = new RegistrationForm
                    {
                        Id = Guid.NewGuid(),
                        ExternalIdentifier = id,
                        Title = googleForm.Title,
                        State = State.Setup
                    };
                    context.RegistrationForms.Add(form);
                }

                // update questions
                var existingQuestions = await context.Questions.Where(qst => qst.RegistrationFormId == form.Id).ToListAsync();
                foreach (var receivedQuestion in googleForm.Questions)
                {
                    var existingQuestion = existingQuestions.FirstOrDefault(qst => qst.ExternalId == receivedQuestion.Id);
                    if (existingQuestion == null)
                    {
                        // new question
                        var question = new Question
                        {
                            Id = Guid.NewGuid(),
                            RegistrationFormId = form.Id,
                            ExternalId = receivedQuestion.Id,
                            Index = receivedQuestion.Index,
                            Title = receivedQuestion.Title,
                            Type = (QuestionType)receivedQuestion.Type
                        };
                        context.Questions.Add(question);
                        if (receivedQuestion.Choices?.Any() == true)
                        {
                            foreach (var choice in receivedQuestion.Choices)
                            {
                                context.QuestionOptions.Add(new QuestionOption
                                {
                                    Id = Guid.NewGuid(),
                                    QuestionId = question.Id,
                                    Answer = choice
                                });
                            }
                        }
                    }
                    else
                    {
                        // update existing question
                        existingQuestion.Index = receivedQuestion.Index;
                        existingQuestion.Title = receivedQuestion.Title;
                        existingQuestion.Type = (QuestionType)receivedQuestion.Type;
                        existingQuestions.Remove(existingQuestion);

                        // update options
                        if (receivedQuestion.Choices?.Any() == true)
                        {
                            var existingOptions = await context.QuestionOptions.Where(opt => opt.QuestionId == existingQuestion.Id).ToListAsync();
                            foreach (var receivedChoice in receivedQuestion.Choices)
                            {
                                var existingOption = existingOptions.FirstOrDefault(exo => exo.Answer == receivedChoice);
                                if (existingOption == null)
                                {
                                    context.QuestionOptions.Add(new QuestionOption
                                    {
                                        Id = Guid.NewGuid(),
                                        QuestionId = existingQuestion.Id,
                                        Answer = receivedChoice
                                    });
                                }
                                else
                                {
                                    existingOptions.Remove(existingOption);
                                }
                            }
                            context.QuestionOptions.RemoveRange(existingOptions);
                        }
                    }
                }
                context.Questions.RemoveRange(existingQuestions);

                await context.SaveChangesAsync();
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}