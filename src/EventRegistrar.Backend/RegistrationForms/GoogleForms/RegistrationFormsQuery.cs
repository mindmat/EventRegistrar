using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RegistrationFormsQuery : IRequest<IEnumerable<RegistrationFormItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrationFormsQueryHandler(IQueryable<RawRegistrationForm> _rawForms,
                                           IQueryable<RegistrationForm> _forms,
                                           IQueryable<Event> events)
    : IRequestHandler<RegistrationFormsQuery, IEnumerable<RegistrationFormItem>>
{
    public async Task<IEnumerable<RegistrationFormItem>> Handle(RegistrationFormsQuery query,
                                                                CancellationToken cancellationToken)
    {
        var @event = await events.FirstAsync(evt => evt.Id == query.EventId, cancellationToken);
        var rawFormsData = await _rawForms.Where(frm => frm.EventAcronym == @event.Acronym)
                                          .Select(frm => new { frm.FormExternalIdentifier, frm.Processed, frm.Created, frm.Id })
                                          .ToListAsync(cancellationToken);

        var rawForms = rawFormsData.GroupBy(frm => frm.FormExternalIdentifier)
                                   .Select(grp => new
                                                  {
                                                      ExternalIdentifier = grp.Key,
                                                      PendingRawForm = grp.Where(frm => frm.Processed == null)
                                                                          .MaxBy(frm => frm.Created),
                                                      LastProcessedRawForm = grp.Where(frm => frm.Processed != null)
                                                                                .MaxBy(frm => frm.Created)
                                                  })
                                   .ToList();

        var formsData = await _forms.Where(frm => frm.EventId == query.EventId)
                                    .Select(frm => new
                                                   {
                                                       FormId = frm.Id,
                                                       frm.ExternalIdentifier,
                                                       frm.State,
                                                       frm.Title,
                                                       EventState = frm.Event!.State,
                                                       FormState = frm.State,
                                                       Questions = frm.Questions!
                                                                      .Select(qst => new
                                                                                     {
                                                                                         qst.Id,
                                                                                         qst.Section,
                                                                                         qst.Index,
                                                                                         qst.Title,
                                                                                         qst.Type,
                                                                                         qst.Mapping,
                                                                                         qst.TemplateKey,
                                                                                         Options = qst.QuestionOptions!
                                                                                                      .Select(qop => new
                                                                                                                     {
                                                                                                                         qop.Id,
                                                                                                                         qop.Answer,
                                                                                                                         MappedRegistrables =
                                                                                                                             qop.Mappings!
                                                                                                                                .Select(map => new
                                                                                                                                        {
                                                                                                                                            map.RegistrableId,
                                                                                                                                            map.Type,
                                                                                                                                            map.Language
                                                                                                                                        })
                                                                                                                     })
                                                                                     }),
                                                       frm.MultiMappings
                                                   })
                                    .ToListAsync(cancellationToken);

        var forms = new List<RegistrationFormItem>();
        foreach (var rawForm in rawForms)
        {
            var existingForm = formsData.FirstOrDefault(frm => frm.ExternalIdentifier == rawForm.ExternalIdentifier);
            if (existingForm != null)
            {
                var formItem = new RegistrationFormItem
                               {
                                   RegistrationFormId = existingForm.FormId,
                                   ExternalIdentifier = existingForm.ExternalIdentifier,
                                   State = existingForm.FormState,
                                   Title = existingForm.Title,
                                   LastImport = rawForm.LastProcessedRawForm?.Created,
                                   PendingRawFormId = rawForm.PendingRawForm?.Id,
                                   PendingRawFormCreated = rawForm.PendingRawForm?.Created,
                                   Deletable = existingForm.FormState == EventState.Setup
                                            && existingForm.EventState == EventState.Setup,
                                   Sections = existingForm.Questions
                                                          .GroupBy(qst => qst.Section)
                                                          .Select(grp => new FormSection
                                                                         {
                                                                             Name = grp.Key,
                                                                             SortKey = grp.Min(qst => qst.Index),
                                                                             Questions = grp.Where(qst => qst.Type != QuestionType.SectionHeader
                                                                                                       && qst.Type != QuestionType.PageBreak)
                                                                                            .Select(qst => new QuestionMappingDisplayItem
                                                                                                           {
                                                                                                               Id = qst.Id,
                                                                                                               Question = qst.Title,
                                                                                                               Type = qst.Type,
                                                                                                               SortKey = qst.Index,
                                                                                                               Mappable = qst.Type is QuestionType.Text
                                                                                                                              or QuestionType.ParagraphText,
                                                                                                               Mapping = qst.Mapping,
                                                                                                               AllowMailTemplateKey = qst.Type is QuestionType.Checkbox
                                                                                                                       or QuestionType.MultipleChoice
                                                                                                                       or QuestionType.Text
                                                                                                                && qst.Mapping == null,
                                                                                                               ShowMailTemplateKey = qst.TemplateKey != null,
                                                                                                               MailTemplateKey = qst.TemplateKey,
                                                                                                               Options = qst.Options.Select(qop =>
                                                                                                                       new QuestionOptionMappingDisplayItem
                                                                                                                       {
                                                                                                                           Id = qop.Id,
                                                                                                                           Answer = qop.Answer,
                                                                                                                           MappedRegistrableCombinedIds = qop.MappedRegistrables
                                                                                                                               .Select(map => new CombinedMappingId(
                                                                                                                                       map.Type,
                                                                                                                                       map.RegistrableId,
                                                                                                                                       map.Language).ToString())
                                                                                                                       })
                                                                                                           })
                                                                                            .OrderBy(qst => qst.SortKey)
                                                                         })
                                                          .Where(sec => sec.Questions.Any())
                                                          .OrderBy(sec => sec.SortKey),
                                   MultiMappings = existingForm.MultiMappings!
                                                               .OrderBy(mqm => mqm.SortKey)
                                                               .Select(mqm => new MultiMapping
                                                                              {
                                                                                  Id = mqm.Id,
                                                                                  QuestionOptionIds = mqm.QuestionOptionIds,
                                                                                  RegistrableCombinedIds = mqm.RegistrableCombinedIds,
                                                                                  SortKey = mqm.SortKey
                                                                              })
                               };
                forms.Add(formItem);
            }
            else if (rawForm.PendingRawForm?.FormExternalIdentifier != null)
            {
                forms.Add(new RegistrationFormItem
                          {
                              ExternalIdentifier = rawForm.PendingRawForm.FormExternalIdentifier,
                              PendingRawFormId = rawForm.PendingRawForm?.Id,
                              PendingRawFormCreated = rawForm.PendingRawForm?.Created
                          });
            }
        }

        return forms;
    }
}

public record MultiMapping
{
    public Guid Id { get; set; }
    public IEnumerable<Guid> QuestionOptionIds { get; set; } = null!;
    public IEnumerable<string> RegistrableCombinedIds { get; set; } = null!;
    public int SortKey { get; set; }
}