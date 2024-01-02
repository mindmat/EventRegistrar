using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Mailing.Import;

public class ExternalMailConfigurationQuery : IRequest<IEnumerable<ExternalMailConfigurationDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ExternalMailConfigurationDisplayItem
{
    public Guid Id { get; set; }
    public string? ImapHost { get; set; }
    public int ImapPort { get; set; }
    public string? Username { get; set; }
    public bool PasswordSet { get; set; }
    public DateTime? ImportMailsSince { get; set; }

    public bool? CheckSuccessful { get; set; }
    public string? CheckError { get; set; }
}

public class ExternalMailConfigurationQueryHandler(ConfigurationRegistry configurationRegistry) : IRequestHandler<ExternalMailConfigurationQuery, IEnumerable<ExternalMailConfigurationDisplayItem>>
{
    public Task<IEnumerable<ExternalMailConfigurationDisplayItem>> Handle(ExternalMailConfigurationQuery query, CancellationToken cancellationToken)
    {
        var config = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(query.EventId)
                                          .MailConfigurations?
                                          .Select(cfg => new ExternalMailConfigurationDisplayItem
                                                         {
                                                             Id = cfg.Id,
                                                             ImapHost = cfg.ImapHost,
                                                             ImapPort = cfg.ImapPort,
                                                             Username = cfg.Username,
                                                             PasswordSet = cfg.Password != null,
                                                             ImportMailsSince = cfg.ImportMailsSince,

                                                             CheckSuccessful = cfg.CheckSuccessful,
                                                             CheckError = cfg.CheckError
                                                         })
                  ?? Enumerable.Empty<ExternalMailConfigurationDisplayItem>();

        return Task.FromResult(config);
    }
}