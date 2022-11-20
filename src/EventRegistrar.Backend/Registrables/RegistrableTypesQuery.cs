using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Registrables;

public class RegistrableTypesQuery : IRequest<IEnumerable<RegistrableTypeOption>>
{
    public Guid EventId { get; set; }
}

public class RegistrableTypesQueryHandler : IRequestHandler<RegistrableTypesQuery, IEnumerable<RegistrableTypeOption>>
{
    private readonly EnumTranslator _enumTranslator;

    public RegistrableTypesQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<RegistrableTypeOption>> Handle(RegistrableTypesQuery query,
                                                           CancellationToken cancellationToken)
    {
        var mappings = _enumTranslator.TranslateAll<RegistrableType>()
                                      .Select(kvp => new RegistrableTypeOption
                                                     {
                                                         Type = kvp.Key,
                                                         Text = kvp.Value
                                                     });
        return Task.FromResult(mappings);
    }
}

public class RegistrableTypeOption
{
    public RegistrableType Type { get; set; }
    public string Text { get; set; } = null!;
}