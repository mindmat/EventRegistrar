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
    public IEnumerable<string>? AvailableLanguages { get; set; }
}

public class UpdateAutoMailConfigurationCommandHandler : IRequestHandler<UpdateAutoMailConfigurationCommand>
{
    private readonly ConfigurationRegistry _configurationRegistry;
    private readonly IEventBus _eventBus;

    public UpdateAutoMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                     IEventBus eventBus)
    {
        _configurationRegistry = configurationRegistry;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateAutoMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var config = _configurationRegistry.GetConfiguration<MailConfiguration>(command.EventId);
        config.SenderName = command.SenderName;
        if (command.SenderMail != null)
        {
            config.SenderMail = command.SenderMail;
        }

        config.SingleRegistrationPossible = command.SingleRegistrationPossible;
        config.PartnerRegistrationPossible = command.PartnerRegistrationPossible;

        if (command.AvailableLanguages?.Any() == true)
        {
            config.AvailableLanguages = command.AvailableLanguages.OrderBy(lng => lng);
        }

        await _configurationRegistry.UpdateConfiguration(command.EventId, config);

        _eventBus.Publish(new ReadModelUpdated
                          {
                              EventId = command.EventId,
                              QueryName = nameof(AutoMailTemplatesQuery)
                          });
        return Unit.Value;
    }
}