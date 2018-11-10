using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class SaveRegistrationFormDefinitionCommandHandler : IRequestHandler<SaveRegistrationFormDefinitionCommand>
    {
        private readonly IQueryable<Event> _events;
        private readonly IRepository<RegistrationForm> _forms;
        private readonly IRepository<QuestionOption> _questionOptions;
        private readonly IRepository<Question> _questions;
        private readonly IRepository<RawRegistrationForm> _rawForms;

        public SaveRegistrationFormDefinitionCommandHandler(IRepository<RegistrationForm> forms,
                                                            IRepository<RawRegistrationForm> rawForms,
                                                            IRepository<Question> questions,
                                                            IRepository<QuestionOption> questionOptions,
                                                            IQueryable<Event> events)
        {
            _forms = forms;
            _rawForms = rawForms;
            _questions = questions;
            _questionOptions = questionOptions;
            _events = events;
        }

        public async Task<Unit> Handle(SaveRegistrationFormDefinitionCommand command, CancellationToken cancellationToken)
        {
            var acronym = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
            var rawForm = await _rawForms.Where(frm => frm.EventAcronym == acronym.Acronym
                                                    && frm.FormExternalIdentifier == command.FormId
                                                    && !frm.Processed)
                                         .OrderByDescending(frm => frm.Created)
                                         .FirstOrDefaultAsync(cancellationToken);
            if (rawForm == null)
            {
                throw new ArgumentException("No unprocessed form found");
            }

            var formDescription = JsonConvert.DeserializeObject<FormDescription>(rawForm.ReceivedMessage);
            var form = await _forms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == command.FormId, cancellationToken);
            if (form != null)
            {
                // update existing form
                if (form.State != State.Setup)
                {
                    throw new InvalidOperationException("Registration form can only be changed in state 'setup'");
                }

                form.Title = formDescription.Title;
            }
            else
            {
                // new form
                form = new RegistrationForm
                {
                    Id = Guid.NewGuid(),
                    EventId = command.EventId,
                    ExternalIdentifier = command.FormId,
                    Title = formDescription.Title,
                    State = State.Setup
                };
                await _forms.InsertOrUpdateEntity(form, cancellationToken);
            }

            // update questions
            var existingQuestions = await _questions.Where(qst => qst.RegistrationFormId == form.Id).ToListAsync(cancellationToken);
            foreach (var receivedQuestion in formDescription.Questions)
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
                        Type = (Questions.QuestionType)receivedQuestion.Type
                    };
                    await _questions.InsertOrUpdateEntity(question, cancellationToken);
                    if (receivedQuestion.Choices?.Any() == true)
                    {
                        foreach (var choice in receivedQuestion.Choices)
                        {
                            await _questionOptions.InsertOrUpdateEntity(new QuestionOption
                            {
                                Id = Guid.NewGuid(),
                                QuestionId = question.Id,
                                Answer = choice
                            }, cancellationToken);
                        }
                    }
                }
                else
                {
                    // update existing question
                    existingQuestion.Index = receivedQuestion.Index;
                    existingQuestion.Title = receivedQuestion.Title;
                    existingQuestion.Type = (Questions.QuestionType)receivedQuestion.Type;
                    existingQuestions.Remove(existingQuestion);

                    // update options
                    if (receivedQuestion.Choices?.Any() == true)
                    {
                        var existingOptions = await _questionOptions.Where(opt => opt.QuestionId == existingQuestion.Id).ToListAsync(cancellationToken);
                        foreach (var receivedChoice in receivedQuestion.Choices)
                        {
                            var existingOption = existingOptions.FirstOrDefault(exo => exo.Answer == receivedChoice);
                            if (existingOption == null)
                            {
                                await _questionOptions.InsertOrUpdateEntity(new QuestionOption
                                {
                                    Id = Guid.NewGuid(),
                                    QuestionId = existingQuestion.Id,
                                    Answer = receivedChoice
                                }, cancellationToken);
                            }
                            else
                            {
                                existingOptions.Remove(existingOption);
                            }
                        }

                        foreach (var existingOption in existingOptions)
                        {
                            _questionOptions.Remove(existingOption);
                        }
                    }
                }
            }
            foreach (var existingQuestion in existingQuestions)
            {
                _questions.Remove(existingQuestion);
            }

            rawForm.Processed = true;

            return Unit.Value;
        }
    }
}