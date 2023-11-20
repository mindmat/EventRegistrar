using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportMailsFromImapForAllActiveEventsCommand : IRequest;

public class ImportMailsFromImapForAllActiveEventsCommandHandler(IQueryable<EventConfiguration> configurations,
                                                                 CommandQueue serviceBus)
    : IRequestHandler<ImportMailsFromImapForAllActiveEventsCommand>
{
    public async Task Handle(ImportMailsFromImapForAllActiveEventsCommand command,
                             CancellationToken cancellationToken)
    {
        var activeImportConfigurations = await configurations.Where(cfg => cfg.Event!.State != EventState.Finished
                                                                        && cfg.Type == typeof(ExternalMailConfigurations).FullName)
                                                             .ToListAsync(cancellationToken);
        foreach (var activeImportConfiguration in activeImportConfigurations)
        {
            serviceBus.EnqueueCommand(new ImportMailsFromImapCommand { EventId = activeImportConfiguration.EventId });
        }
    }
}