using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Import;

namespace EventRegistrar.Backend.Payments.Account;

public class SaveBankAccountConfigurationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public BankAccountConfiguration? Config { get; set; }
}


public class SaveBankAccountConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                 ChangeTrigger changeTrigger)
    : IRequestHandler<SaveBankAccountConfigurationCommand>
{
    public async Task Handle(SaveBankAccountConfigurationCommand command, CancellationToken cancellationToken)
    {
        if (command.Config == null)
        {
            throw new ArgumentNullException(nameof(command.Config));
        }
        await configurationRegistry.UpdateConfiguration(command.EventId,
                                                        command.Config);

        changeTrigger.QueryChanged<ExternalMailConfigurationQuery>(command.EventId);
    }
}