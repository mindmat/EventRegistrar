using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Mailing.Import;

public class SaveExternalMailConfigurationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<ExternalMailConfigurationUpdateItem>? Configs { get; set; }
}

public class ExternalMailConfigurationUpdateItem
{
    public Guid Id { get; set; }
    public string? ImapHost { get; set; }
    public int ImapPort { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public DateTime? ImportMailsSince { get; set; }
}

public class SaveExternalMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                         ChangeTrigger changeTrigger)
    : IRequestHandler<SaveExternalMailConfigurationCommand>
{
    public async Task Handle(SaveExternalMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var existingConfigs = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(command.EventId)
                                                   .MailConfigurations;

        var newConfigs = command.Configs?
                                .Select(nwc =>
                                {
                                    var existing = existingConfigs?.FirstOrDefault(exc => exc.Id == nwc.Id);
                                    var config = new ExternalMailConfiguration
                                                 {
                                                     Id = nwc.Id,
                                                     ImapHost = nwc.ImapHost,
                                                     ImapPort = nwc.ImapPort,
                                                     Username = nwc.Username,
                                                     Password = string.IsNullOrWhiteSpace(nwc.Password)
                                                                    ? existing?.Password
                                                                    : nwc.Password,
                                                     ImportMailsSince = nwc.ImportMailsSince
                                                 };
                                    if (existing is { CheckSuccessful: not null })
                                    {
                                        config.CheckSuccessful = existing.CheckSuccessful;
                                        config.CheckError = existing.CheckError;
                                    }

                                    return config;
                                })
                                .ToList();

        await configurationRegistry.UpdateConfiguration(command.EventId,
                                                        new ExternalMailConfigurations
                                                        {
                                                            MailConfigurations = newConfigs
                                                        });

        changeTrigger.QueryChanged<ExternalMailConfigurationQuery>(command.EventId);
        foreach (var config in newConfigs?.Where(cfg => cfg is { ImapHost: not null, Username: not null })
                            ?? Enumerable.Empty<ExternalMailConfiguration>())
        {
            changeTrigger.EnqueueCommand(new CheckExternalMailConfigurationCommand
                                         {
                                             EventId = command.EventId,
                                             ExternalMailConfigurationId = config.Id
                                         });
        }
    }
}