using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.FormPaths;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class SaveRegistrationFormMappingsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid FormId { get; set; }
    public IEnumerable<FormSection>? Sections { get; set; }
}

public class SaveRegistrationFormMappingsCommandHandler : IRequestHandler<SaveRegistrationFormMappingsCommand>
{
    private readonly IRepository<RegistrationForm> _forms;
    private readonly IRepository<QuestionOptionMapping> _mappings;

    public SaveRegistrationFormMappingsCommandHandler(IRepository<RegistrationForm> forms,
                                                      IRepository<QuestionOptionMapping> mappings)
    {
        _forms = forms;
        _mappings = mappings;
    }

    public async Task<Unit> Handle(SaveRegistrationFormMappingsCommand command, CancellationToken cancellationToken)
    {
        var sectionsToSave = command.Sections;
        if (sectionsToSave == null)
        {
            return Unit.Value;
        }

        var form = await _forms.Where(frm => frm.EventId == command.EventId
                                          && frm.Id == command.FormId)
                               .Include(frm => frm.Questions!)
                               .ThenInclude(qst => qst.QuestionOptions!)
                               .ThenInclude(qop => qop.Mappings)
                               .FirstAsync(cancellationToken);

        foreach (var questionToSave in sectionsToSave.SelectMany(sec => sec.Questions))
        {
            var question = form.Questions.FirstOrDefault(qst => qst.Id == questionToSave.Id);
            if (question == null)
            {
                continue;
            }

            question.Mapping = questionToSave.Mapping;
            foreach (var optionToSave in questionToSave.Options ?? Enumerable.Empty<QuestionOptionMappingDisplayItem>())
            {
                var option = question?.QuestionOptions.FirstOrDefault(qop => qop.Id == optionToSave.Id);
                if (option == null)
                {
                    continue;
                }

                var existingMappings = new List<QuestionOptionMapping>(option.Mappings ?? Enumerable.Empty<QuestionOptionMapping>());

                foreach (var mapping in optionToSave.MappedRegistrableCombinedIds?.Select(cid => new CombinedMappingId(cid)) ?? Enumerable.Empty<CombinedMappingId>())
                {
                    var existingMapping = existingMappings.FirstOrDefault(map => map.Type == mapping.Type
                                                                              && map.RegistrableId == mapping.Id
                                                                              && map.Language == mapping.Language);
                    if (existingMapping == null)
                    {
                        await _mappings.InsertOrUpdateEntity(new QuestionOptionMapping
                                                             {
                                                                 Id = Guid.NewGuid(),
                                                                 QuestionOptionId = option.Id,
                                                                 Type = mapping.Type,
                                                                 RegistrableId = mapping.Id,
                                                                 Language = mapping.Language
                                                             }, cancellationToken);
                    }
                    else
                    {
                        existingMappings.Remove(existingMapping);
                    }
                }

                // Check if option has been removed
                foreach (var removedMapping in existingMappings)
                {
                    _mappings.Remove(removedMapping);
                }
            }
        }

        return Unit.Value;
    }
}