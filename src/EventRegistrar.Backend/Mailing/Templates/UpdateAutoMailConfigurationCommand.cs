using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Templates;

public class UpdateAutoMailConfigurationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? SenderName { get; set; }
    public string? SenderMail { get; set; }
    public bool SingleRegistrationPossible { get; set; }
    public bool PartnerRegistrationPossible { get; set; }
    public bool SendRegistrationReceivedMail { get; set; }
    public IEnumerable<string>? AvailableLanguages { get; set; }
    public MailSender? MailSender { get; set; }

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
}

public class UpdateAutoMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                       IEventBus eventBus)
    : IRequestHandler<UpdateAutoMailConfigurationCommand>
{
    public async Task Handle(UpdateAutoMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var config = configurationRegistry.GetConfiguration<MailConfiguration>(command.EventId);
        config.SenderName = command.SenderName;
        if (command.SenderMail != null)
        {
            config.SenderMail = command.SenderMail;
        }

        config.SingleRegistrationPossible = command.SingleRegistrationPossible;
        config.PartnerRegistrationPossible = command.PartnerRegistrationPossible;
        config.SendRegistrationReceivedMail = command.SendRegistrationReceivedMail;

        if (command.AvailableLanguages?.Any() == true)
        {
            config.AvailableLanguages = command.AvailableLanguages.OrderBy(lng => lng);
        }

        if (command.MailSender != null)
        {
            config.MailSender = command.MailSender.Value;

            if (command.MailSender == MailSender.Smtp)
            {
                config.SmtpConfiguration = new SmtpConfiguration
                                           {
                                               Host = command.SmtpHost,
                                               Port = command.SmtpPort,
                                               Username = command.SmtpUsername,
                                               Password = string.IsNullOrWhiteSpace(command.SmtpPassword)
                                                              ? config.SmtpConfiguration?.Password
                                                              : command.SmtpPassword
                                           };
            }
        }


        await configurationRegistry.UpdateConfiguration(command.EventId, config);

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(AutoMailTemplatesQuery)
                         });
    }
}