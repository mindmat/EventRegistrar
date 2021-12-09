using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class SaveRegistrationFormMappingsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public RegistrationFormGroup Mappings { get; set; } = null!;
    public Guid FormId { get; set; }
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
        var formToSave = command.Mappings;
        if (formToSave == null) return Unit.Value;

        var form = await _forms.Where(frm => frm.EventId == command.EventId
                                          && frm.Id == formToSave.Id)
                               .Include(frm => frm.Questions)
                               .ThenInclude(qst => qst.QuestionOptions)
                               .ThenInclude(qop => qop.Mappings)
                               .FirstAsync(cancellationToken);

        foreach (var questionToSave in formToSave.Sections.SelectMany(sec => sec.Questions))
        {
            var question = form.Questions.FirstOrDefault(qst => qst.Id == questionToSave.Id);
            if (question == null) continue;

            question.Mapping = questionToSave.Mapping;
            foreach (var optionToSave in questionToSave.Options ?? Enumerable.Empty<QuestionOptionMappingDisplayItem>())
            {
                var option = question?.QuestionOptions.FirstOrDefault(qop => qop.Id == optionToSave.Id);
                if (option == null) continue;

                var existingMappings =
                    new List<QuestionOptionMapping>(option.Mappings ?? Enumerable.Empty<QuestionOptionMapping>());

                foreach (var mapping in optionToSave.MappedRegistrables ??
                                        Enumerable.Empty<AvailableQuestionOptionMapping>())
                {
                    if (mapping.Type == null || mapping.Id == null || mapping.Language == null)
                    {
                        var splits = mapping.CombinedId.Split('/').ToList();
                        if (splits.Count >= 3)
                        {
                            if (Guid.TryParse(splits[0], out var id)) mapping.Id = id;

                            if (Enum.TryParse<MappingType>(splits[1], out var type)) mapping.Type = type;
                            var language = splits[2];
                            if (!string.IsNullOrWhiteSpace(language)) mapping.Language = language;
                        }
                    }

                    var existingMapping = existingMappings.FirstOrDefault(map => map.Type == mapping.Type
                     && map.RegistrableId == mapping.Id
                     && map.Language == mapping.Language);
                    if (existingMapping == null)
                        await _mappings.InsertOrUpdateEntity(new QuestionOptionMapping
                                                             {
                                                                 Id = Guid.NewGuid(),
                                                                 QuestionOptionId = option.Id,
                                                                 Type = mapping.Type,
                                                                 RegistrableId = mapping.Id,
                                                                 Language = mapping.Language
                                                             }, cancellationToken);
                    else
                        existingMappings.Remove(existingMapping);
                }

                // Check if option has been removed
                foreach (var removedMapping in existingMappings) _mappings.Remove(removedMapping);
            }
        }

        return Unit.Value;
    }
}