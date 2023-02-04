﻿using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrables;

public class SaveRegistrableCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public string? Name { get; set; }
    public string? NameSecondary { get; set; }
    public RegistrableType Type { get; set; }
    public int? MaximumSingleSpots { get; set; }
    public int? MaximumDoubleSpots { get; set; }
    public int? MaximumAllowedImbalance { get; set; }
    public bool HasWaitingList { get; set; }
    public string? Tag { get; set; }
}

public class SaveRegistrableCommandHandler : AsyncRequestHandler<SaveRegistrableCommand>
{
    private readonly IRepository<Registrable> _registrables;
    private readonly IQueryable<Event> _events;
    private readonly ReadModelUpdater _readModelUpdater;
    private readonly EnumTranslator _enumTranslator;

    public SaveRegistrableCommandHandler(IRepository<Registrable> registrables,
                                         IQueryable<Event> events,
                                         ReadModelUpdater readModelUpdater,
                                         EnumTranslator enumTranslator)
    {
        _registrables = registrables;
        _events = events;
        _readModelUpdater = readModelUpdater;
        _enumTranslator = enumTranslator;
    }

    protected override async Task Handle(SaveRegistrableCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentNullException(nameof(SaveRegistrableCommand.Name));
        }

        var @event = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        var registrable = await _registrables.AsTracking()
                                             .Include(rbl => rbl.Spots)
                                             .FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableId, cancellationToken);
        if (registrable == null)
        {
            registrable = _registrables.InsertObjectTree(new Registrable
                                                         {
                                                             Id = command.RegistrableId,
                                                             EventId = command.EventId,
                                                             Type = command.Type
                                                         });
        }
        else
        {
            if (registrable.EventId != command.EventId)
            {
                throw new InvalidOperationException("Invalid EventId");
            }

            if (registrable.Type != command.Type)
            {
                if (@event.State != RegistrationForms.EventState.Setup)
                {
                    throw new Exception($"To change the type of a track, event must be in state setup, but it is in state {_enumTranslator.Translate(@event.State)}");
                }

                var hasSpots = registrable.Spots?.Any(spt => !spt.IsCancelled) == true;
                if (hasSpots)
                {
                    throw new Exception($"To change the type of a track, there must not be any registrations in it, but there are {registrable.Spots?.Count}");
                }

                var hasSpotsOnWaitingList = registrable.Spots?.Any(spt => spt is { IsCancelled: false, IsWaitingList: true }) == true;
                if (registrable.HasWaitingList && !command.HasWaitingList && hasSpotsOnWaitingList)
                {
                    throw new Exception("Waiting list cannot be removed because there are spots on the waiting list");
                }


                registrable.Type = command.Type;
            }
        }

        registrable.Name = command.Name;
        registrable.NameSecondary = command.NameSecondary;
        registrable.DisplayName = Enumerable.Empty<string?>()
                                            .Append(registrable.Name)
                                            .Append(registrable.NameSecondary)
                                            .StringJoinNullable(" - ");
        registrable.Tag = command.Tag;

        switch (registrable.Type)
        {
            case RegistrableType.Single:
                registrable.MaximumSingleSeats = command.MaximumSingleSpots;
                registrable.MaximumDoubleSeats = null;
                registrable.MaximumAllowedImbalance = null;
                registrable.HasWaitingList = command is { HasWaitingList: true, MaximumSingleSpots: { } };
                break;

            case RegistrableType.Double:
                registrable.MaximumSingleSeats = null;
                registrable.MaximumDoubleSeats = command.MaximumDoubleSpots;
                registrable.MaximumAllowedImbalance = command.MaximumAllowedImbalance;
                registrable.HasWaitingList = command is { HasWaitingList: true, MaximumDoubleSpots: { } };
                break;
        }

        _readModelUpdater.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
    }
}