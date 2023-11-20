using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class AvailableQuestionMappingsQuery : IRequest<IEnumerable<AvailableQuestionMapping>>
{
    public Guid EventId { get; set; }
}

public class AvailableQuestionMappingsQueryHandler(EnumTranslator enumTranslator) : IRequestHandler<AvailableQuestionMappingsQuery, IEnumerable<AvailableQuestionMapping>>
{
    public Task<IEnumerable<AvailableQuestionMapping>> Handle(AvailableQuestionMappingsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var mappings = enumTranslator.TranslateAll<QuestionMappingType>()
                                     .Select(kvp => new AvailableQuestionMapping
                                                    {
                                                        Type = kvp.Key,
                                                        Text = kvp.Value
                                                    });
        return Task.FromResult(mappings);
    }
}

public class AvailableQuestionMapping
{
    public QuestionMappingType Type { get; set; }
    public string Text { get; set; } = null!;
}