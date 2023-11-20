using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations.Responses;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class ImportRegistrationFormCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string FormExternalIdentifier { get; set; }
}

public class ImportRegistrationFormCommandHandler(IRepository<RegistrationForm> forms,
                                                  IRepository<RawRegistrationForm> rawForms,
                                                  IRepository<Question> questions,
                                                  IRepository<QuestionOption> questionOptions,
                                                  IRepository<Response> responses,
                                                  IQueryable<Event> events,
                                                  IDateTimeProvider dateTimeProvider,
                                                  IEventBus eventBus)
    : IRequestHandler<ImportRegistrationFormCommand>
{
    public async Task Handle(ImportRegistrationFormCommand command, CancellationToken cancellationToken)
    {
        var acronym = await events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        var rawForm = await rawForms.Where(frm => frm.EventAcronym == acronym.Acronym
                                               && frm.FormExternalIdentifier == command.FormExternalIdentifier
                                               && frm.Processed == null)
                                    .OrderByDescending(frm => frm.Created)
                                    .FirstOrDefaultAsync(cancellationToken);
        if (rawForm == null)
        {
            throw new ArgumentException("No unprocessed form found");
        }

        var formDescription = JsonConvert.DeserializeObject<FormDescription>(rawForm.ReceivedMessage);
        var form = await forms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == command.FormExternalIdentifier, cancellationToken);
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
            await forms.InsertOrUpdateEntity(form, cancellationToken);
        }

        // update questions
        var existingQuestions = await questions.Where(qst => qst.RegistrationFormId == form.Id)
                                               .Include(qst => qst.Responses)
                                               .AsTracking()
                                               .ToListAsync(cancellationToken);
        string section = null;
        foreach (var receivedQuestion in formDescription.Questions.OrderBy(que => que.Index))
        {
            var type = (QuestionType)receivedQuestion.Type;
            if (type is QuestionType.SectionHeader
                or QuestionType.PageBreak)
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
                questions.InsertObjectTree(question);
                if (receivedQuestion.Choices?.Any() == true)
                {
                    foreach (var choice in receivedQuestion.Choices)
                    {
                        questionOptions.InsertObjectTree(new QuestionOption
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
                    var existingOptions = await questionOptions.Where(opt => opt.QuestionId == existingQuestion.Id)
                                                               .ToListAsync(cancellationToken);
                    foreach (var receivedChoice in receivedQuestion.Choices)
                    {
                        var existingOption = existingOptions.FirstOrDefault(exo => exo.Answer == receivedChoice);
                        if (existingOption == null)
                        {
                            questionOptions.InsertObjectTree(new QuestionOption
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
                        questionOptions.Remove(existingOption);
                        existingOption.Responses?.ForEach(rsp => responses.Remove(rsp)); // "cascaded delete"
                    }
                }
            }
        }

        // Remove the questions that were not delivered anymore
        foreach (var existingQuestion in existingQuestions)
        {
            questions.Remove(existingQuestion);
            existingQuestion.Responses?.ForEach(rsp => responses.Remove(rsp)); // "cascaded delete"
        }

        rawForm.Processed = dateTimeProvider.Now;
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(RegistrationFormsQuery)
                         });
    }
}