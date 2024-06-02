using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Payments.Account;

public class BankAccountConfigurationQuery: IRequest<BankAccountConfiguration>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class BankAccountConfigurationQueryHandler(ConfigurationRegistry _configurationRegistry)
    : IRequestHandler<BankAccountConfigurationQuery, BankAccountConfiguration>
{
    public Task<BankAccountConfiguration> Handle(BankAccountConfigurationQuery query, CancellationToken _)
    {
        return Task.FromResult(_configurationRegistry.GetConfiguration<BankAccountConfiguration>(query.EventId));
    }
}