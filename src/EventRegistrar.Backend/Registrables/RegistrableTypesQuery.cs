using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Registrables;

public class RegistrableTypesQuery : IRequest<IEnumerable<RegistrableTypeOption>>
{
    public Guid EventId { get; set; }
}

public class RegistrableTypesQueryHandler(EnumTranslator enumTranslator) : IRequestHandler<RegistrableTypesQuery, IEnumerable<RegistrableTypeOption>>
{
    public Task<IEnumerable<RegistrableTypeOption>> Handle(RegistrableTypesQuery query,
                                                           CancellationToken cancellationToken)
    {
        var mappings = enumTranslator.TranslateAll<RegistrableType>()
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