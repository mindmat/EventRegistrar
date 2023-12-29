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
    public string? ImapHost { get; set; }
    public int ImapPort { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class SaveExternalMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                         ChangeTrigger changeTrigger) : IRequestHandler<SaveExternalMailConfigurationCommand>
{
    public async Task Handle(SaveExternalMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var existingConfigs = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(command.EventId)
                                                   .MailConfigurations;

        var newConfigs = command.Configs?.Select(nwc =>
        {
            var existing = existingConfigs?.FirstOrDefault(exc => exc.ImapHost == nwc.ImapHost
                                                               && exc.Username == nwc.Username);
            return new ExternalMailConfiguration
                   {
                       ImapHost = nwc.ImapHost,
                       ImapPort = nwc.ImapPort,
                       Username = nwc.Username,
                       Password = string.IsNullOrWhiteSpace(nwc.Password)
                                      ? existing?.Password
                                      : nwc.Password
                   };
        });

        await configurationRegistry.UpdateConfiguration(command.EventId,
                                                        new ExternalMailConfigurations
                                                        {
                                                            MailConfigurations = newConfigs
                                                        });

        changeTrigger.QueryChanged<ExternalMailConfigurationQuery>(command.EventId);
    }
}