using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

using MailKit.Net.Imap;
using MailKit;

namespace EventRegistrar.Backend.Mailing.Import;

public class CheckExternalMailConfigurationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ExternalMailConfigurationId { get; set; }
}

public class CheckExternalMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                          ChangeTrigger changeTrigger) : IRequestHandler<CheckExternalMailConfigurationCommand>
{
    public async Task Handle(CheckExternalMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var mailConfigurations = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(command.EventId);
        var config = mailConfigurations.MailConfigurations?.FirstOrDefault(cfg => cfg.Id == command.ExternalMailConfigurationId);
        if (config == null)
        {
            return;
        }

        using var client = new ImapClient();
        client.ServerCertificateValidationCallback = (sender,
                                                      certificate,
                                                      chain,
                                                      errors) => true;
        try
        {
            await client.ConnectAsync(config.ImapHost, config.ImapPort, true, cancellationToken);
            try
            {
                await client.AuthenticateAsync(config.Username, config.Password, cancellationToken);
                try
                {
                    // The Inbox folder is always available on all IMAP servers
                    await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
                    config.CheckSuccessful = true;
                    config.CheckError = null;
                }
                catch (Exception ex)
                {
                    config.CheckSuccessful = false;
                    config.CheckError = $"Could not open inbox. Error: {ex.Message}";
                    await configurationRegistry.UpdateConfiguration(command.EventId, mailConfigurations);
                }
            }
            catch (Exception ex)
            {
                config.CheckSuccessful = false;
                config.CheckError = $"Could not authenticate user {config.Username}. Error: {ex.Message}";
                await configurationRegistry.UpdateConfiguration(command.EventId, mailConfigurations);
            }
        }
        catch (Exception ex)
        {
            config.CheckSuccessful = false;
            config.CheckError = $"Could not connect to {config.ImapHost}:{config.ImapPort}. Error: {ex.Message}";
            await configurationRegistry.UpdateConfiguration(command.EventId, mailConfigurations);
        }

        await configurationRegistry.UpdateConfiguration(command.EventId, mailConfigurations);

        changeTrigger.QueryChanged<ExternalMailConfigurationQuery>(command.EventId);
    }
}