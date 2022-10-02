﻿using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.RegistrationForms;

using MediatR;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportMailsFromImapForAllActiveEventsCommand : IRequest { }

public class ImportMailsFromImapForAllActiveEventsCommandHandler : IRequestHandler<ImportMailsFromImapForAllActiveEventsCommand>
{
    private readonly IQueryable<EventConfiguration> _configurations;
    private readonly CommandQueue _serviceBus;

    public ImportMailsFromImapForAllActiveEventsCommandHandler(IQueryable<EventConfiguration> configurations,
                                                               CommandQueue serviceBus)
    {
        _configurations = configurations;
        _serviceBus = serviceBus;
    }

    public async Task<Unit> Handle(ImportMailsFromImapForAllActiveEventsCommand command,
                                   CancellationToken cancellationToken)
    {
        var activeImportConfigurations = await _configurations.Where(cfg => cfg.Event!.State != EventState.Finished
                                                                         && cfg.Type == typeof(ExternalMailConfigurations).FullName)
                                                              .ToListAsync(cancellationToken);
        foreach (var activeImportConfiguration in activeImportConfigurations)
        {
            _serviceBus.EnqueueCommand(new ImportMailsFromImapCommand { EventId = activeImportConfiguration.EventId });
        }

        return Unit.Value;
    }
}