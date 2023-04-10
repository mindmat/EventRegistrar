using EventRegistrar.Backend.RegistrationForms.GoogleForms;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class SaveRegistrationFormMappingsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid FormId { get; set; }
    public IEnumerable<FormSection>? Sections { get; set; }
    public IEnumerable<MultiMapping>? MultiMappings { get; set; }
}

public class SaveRegistrationFormMappingsCommandHandler : AsyncRequestHandler<SaveRegistrationFormMappingsCommand>
{
    private readonly IRepository<RegistrationForm> _forms;
    private readonly IRepository<QuestionOptionMapping> _mappings;
    private readonly IRepository<MultiQuestionOptionMapping> _multiMappings;

    public SaveRegistrationFormMappingsCommandHandler(IRepository<RegistrationForm> forms,
                                                      IRepository<QuestionOptionMapping> mappings,
                                                      IRepository<MultiQuestionOptionMapping> multiMappings)
    {
        _forms = forms;
        _mappings = mappings;
        _multiMappings = multiMappings;
    }

    protected override async Task Handle(SaveRegistrationFormMappingsCommand command, CancellationToken cancellationToken)
    {
        var sectionsToSave = command.Sections;
        var multiMappingsToSave = (command.MultiMappings ?? Enumerable.Empty<MultiMapping>()).ToList();
        if (sectionsToSave == null)
        {
            return;
        }

        var form = await _forms.AsTracking()
                               .Where(frm => frm.EventId == command.EventId
                                          && frm.Id == command.FormId)
                               .Include(frm => frm.Questions!)
                               .ThenInclude(qst => qst.QuestionOptions!)
                               .ThenInclude(qop => qop.Mappings)
                               .Include(frm => frm.MultiMappings)
                               .FirstAsync(cancellationToken);

        foreach (var questionToSave in sectionsToSave.SelectMany(sec => sec.Questions))
        {
            var question = form.Questions!.FirstOrDefault(qst => qst.Id == questionToSave.Id);
            if (question == null)
            {
                continue;
            }

            question.Mapping = questionToSave.Mapping;
            foreach (var optionToSave in questionToSave.Options ?? Enumerable.Empty<QuestionOptionMappingDisplayItem>())
            {
                var option = question?.QuestionOptions!.FirstOrDefault(qop => qop.Id == optionToSave.Id);
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

        // remove deleted multi mappings
        var multiMappingsToDelete = form.MultiMappings!.ExceptBy(multiMappingsToSave.Select(mqm => mqm.Id), mqm => mqm.Id).ToList();
        foreach (var multiMappingToDelete in multiMappingsToDelete)
        {
            _multiMappings.Remove(multiMappingToDelete);
        }

        // upsert multi mappings
        foreach (var multiMappingToSave in multiMappingsToSave)
        {
            var multiMapping = form.MultiMappings!.FirstOrDefault(mqm => mqm.Id == multiMappingToSave.Id)
                            ?? _multiMappings.InsertObjectTree(new MultiQuestionOptionMapping
                                                               {
                                                                   Id = multiMappingToSave.Id,
                                                                   RegistrationFormId = form.Id
                                                               });
            multiMapping.QuestionOptionIds = multiMappingToSave.QuestionOptionIds.ToList();
            multiMapping.RegistrableCombinedIds = multiMappingToSave.RegistrableCombinedIds.ToList();
            multiMapping.SortKey = multiMappingToSave.SortKey;
        }
    }
}