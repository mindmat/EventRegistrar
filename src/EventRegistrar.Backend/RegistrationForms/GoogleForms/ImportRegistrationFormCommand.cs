﻿using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class ImportRegistrationFormCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string FormExternalIdentifier { get; set; }
}

public class ImportRegistrationFormCommandHandler : IRequestHandler<ImportRegistrationFormCommand>
{
    private readonly IQueryable<Event> _events;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRepository<RegistrationForm> _forms;
    private readonly IRepository<QuestionOption> _questionOptions;
    private readonly IRepository<Question> _questions;
    private readonly IRepository<RawRegistrationForm> _rawForms;

    public ImportRegistrationFormCommandHandler(IRepository<RegistrationForm> forms,
                                                IRepository<RawRegistrationForm> rawForms,
                                                IRepository<Question> questions,
                                                IRepository<QuestionOption> questionOptions,
                                                IQueryable<Event> events,
                                                IDateTimeProvider dateTimeProvider)
    {
        _forms = forms;
        _rawForms = rawForms;
        _questions = questions;
        _questionOptions = questionOptions;
        _events = events;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(ImportRegistrationFormCommand command, CancellationToken cancellationToken)
    {
        var acronym = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        var rawForm = await _rawForms.Where(frm => frm.EventAcronym == acronym.Acronym
                                                && frm.FormExternalIdentifier == command.FormExternalIdentifier
                                                && frm.Processed == null)
                                     .OrderByDescending(frm => frm.Created)
                                     .FirstOrDefaultAsync(cancellationToken);
        if (rawForm == null)
        {
            throw new ArgumentException("No unprocessed form found");
        }

        var formDescription = JsonConvert.DeserializeObject<FormDescription>(rawForm.ReceivedMessage);
        var form = await _forms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == command.FormExternalIdentifier, cancellationToken);
        if (form != null)
        {
            // update existing form
            if (form.State != EventState.Setup)
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
                       ExternalIdentifier = command.FormExternalIdentifier,
                       Title = formDescription.Title,
                       State = EventState.Setup
                   };
            await _forms.InsertOrUpdateEntity(form, cancellationToken);
        }

        // update questions
        var existingQuestions = await _questions.Where(qst => qst.RegistrationFormId == form.Id)
                                                .AsTracking()
                                                .ToListAsync(cancellationToken);
        string section = null;
        foreach (var receivedQuestion in formDescription.Questions.OrderBy(que => que.Index))
        {
            var type = (Questions.QuestionType)receivedQuestion.Type;
            if (type is Questions.QuestionType.SectionHeader
                or Questions.QuestionType.PageBreak)
            {
                section = receivedQuestion.Title;
            }

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
                                   Type = type,
                                   Section = section
                               };
                _questions.InsertObjectTree(question);
                if (receivedQuestion.Choices?.Any() == true)
                {
                    foreach (var choice in receivedQuestion.Choices)
                    {
                        _questionOptions.InsertObjectTree(new QuestionOption
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
                existingQuestion.Section = section;
                existingQuestion.Index = receivedQuestion.Index;
                existingQuestion.Title = receivedQuestion.Title;
                existingQuestion.Type = type;
                existingQuestions.Remove(existingQuestion);

                // update options
                if (receivedQuestion.Choices?.Any() == true)
                {
                    var existingOptions = await _questionOptions.Where(opt => opt.QuestionId == existingQuestion.Id)
                                                                .ToListAsync(cancellationToken);
                    foreach (var receivedChoice in receivedQuestion.Choices)
                    {
                        var existingOption = existingOptions.FirstOrDefault(exo => exo.Answer == receivedChoice);
                        if (existingOption == null)
                        {
                            _questionOptions.InsertObjectTree(new QuestionOption
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

        rawForm.Processed = _dateTimeProvider.Now;

        return Unit.Value;
    }
}