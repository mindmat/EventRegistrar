namespace EventRegistrar.Backend.Registrations.Raw;

public class AllExternalRegistrationIdentifiersQuery : IRequest<IEnumerable<string>>
{
    public string? RegistrationFormExternalIdentifier { get; set; }
}

public class AllExternalRegistrationIdentifiersQueryHandler(IQueryable<RawRegistration> rawRegistrations,
                                                            ILogger log)
    : IRequestHandler<AllExternalRegistrationIdentifiersQuery, IEnumerable<string>>
{
    private readonly ILogger _log = log;

    public async Task<IEnumerable<string>> Handle(AllExternalRegistrationIdentifiersQuery query,
                                                  CancellationToken cancellationToken)
    {
        var ids = await rawRegistrations.Where(reg => reg.FormExternalIdentifier == query.RegistrationFormExternalIdentifier)
                                        .Select(reg => reg.RegistrationExternalIdentifier)
                                        .ToListAsync(cancellationToken);
        return ids;
    }
}