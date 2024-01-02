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
    public int TotalImportedMails { get; set; }
    public int TotalAssignedMails { get; set; }
}

public class ExternalMailConfigurationQueryHandler(ConfigurationRegistry configurationRegistry,
                                                   IQueryable<ImportedMail> importedMails)
    : IRequestHandler<ExternalMailConfigurationQuery, IEnumerable<ExternalMailConfigurationDisplayItem>>
{
    public async Task<IEnumerable<ExternalMailConfigurationDisplayItem>> Handle(ExternalMailConfigurationQuery query, CancellationToken cancellationToken)
    {
        var configs = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(query.EventId)
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
                                           .ToList()
                   ?? new List<ExternalMailConfigurationDisplayItem>();

        var configIds = configs.Select(cfg => (Guid?)cfg.Id)
                               .ToList();
        var imported = await importedMails.Where(iml => configIds.Contains(iml.ExternalMailConfigurationId))
                                          .Select(iml => new
                                                         {
                                                             iml.ExternalMailConfigurationId,
                                                             Assigned = iml.Registrations!.Any()
                                                         })
                                          .ToListAsync(cancellationToken);
        var importedCounts = imported.GroupBy(iml => iml.ExternalMailConfigurationId)
                                     .Select(grp => new
                                                    {
                                                        Id = grp.Key,
                                                        Total = grp.Count(),
                                                        Assigned = grp.Count(iml => iml.Assigned)
                                                    });
        foreach (var config in configs)
        {
            var counts = importedCounts.FirstOrDefault(cnt => cnt.Id == config.Id);
            config.TotalImportedMails = counts?.Total ?? 0;
            config.TotalAssignedMails = counts?.Assigned ?? 0;
        }

        return configs;
    }
}