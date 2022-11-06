using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class AvailableQuestionMappingsQuery : IRequest<IEnumerable<AvailableQuestionMapping>>
{
    public Guid EventId { get; set; }
}

public class AvailableQuestionMappingsQueryHandler : IRequestHandler<AvailableQuestionMappingsQuery, IEnumerable<AvailableQuestionMapping>>
{
    private readonly EnumTranslator _enumTranslator;

    public AvailableQuestionMappingsQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<AvailableQuestionMapping>> Handle(AvailableQuestionMappingsQuery request,
                                                              CancellationToken cancellationToken)
    {
        var mappings = _enumTranslator.TranslateAll<QuestionMappingType>()
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