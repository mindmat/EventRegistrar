namespace EventRegistrar.Backend.Registrations.Raw;

public class AllExternalRegistrationIdentifiersQuery : IRequest<IEnumerable<string>>
{
    public string? RegistrationFormExternalIdentifier { get; set; }
}

public class AllExternalRegistrationIdentifiersQueryHandler : IRequestHandler<AllExternalRegistrationIdentifiersQuery, IEnumerable<string>>
{
    private readonly ILogger _log;
    private readonly IQueryable<RawRegistration> _rawRegistrations;

    public AllExternalRegistrationIdentifiersQueryHandler(IQueryable<RawRegistration> rawRegistrations,
                                                          ILogger log)
    {
        _rawRegistrations = rawRegistrations;
        _log = log;
    }

    public async Task<IEnumerable<string>> Handle(AllExternalRegistrationIdentifiersQuery query,
                                                  CancellationToken cancellationToken)
    {
        var ids = await _rawRegistrations.Where(reg => reg.FormExternalIdentifier == query.RegistrationFormExternalIdentifier)
                                         .Select(reg => reg.RegistrationExternalIdentifier)
                                         .ToListAsync(cancellationToken);
        return ids;
    }
}