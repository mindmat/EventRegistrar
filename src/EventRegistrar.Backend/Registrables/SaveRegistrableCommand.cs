using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

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

public class SaveRegistrableCommandHandler : IRequestHandler<SaveRegistrableCommand>
{
    private readonly IRepository<Registrable> _registrables;
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IQueryable<Event> _events;

    public SaveRegistrableCommandHandler(IRepository<Registrable> registrables,
                                         CommandQueue commandQueue,
                                         IDateTimeProvider dateTimeProvider,
                                         IQueryable<Event> events)
    {
        _registrables = registrables;
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _events = events;
    }

    public async Task<Unit> Handle(SaveRegistrableCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentNullException(nameof(SaveRegistrableCommand.Name));
        }

        var registrable = await _registrables.AsTracking()
                                             .Include(rbl => rbl.Spots)
                                             .FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableId, cancellationToken)
                       ?? _registrables.InsertObjectTree(new Registrable
                                                         {
                                                             Id = command.RegistrableId,
                                                             EventId = command.EventId
                                                         });
        if (registrable.EventId != command.EventId)
        {
            throw new InvalidOperationException("Invalid EventID");
        }

        var @event = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (@event.State is not RegistrationForms.EventState.Setup and not RegistrationForms.EventState.RegistrationOpen)
        {
            throw new Exception($"To change a registrable, event must be in state setup or open, but it is in state {@event.State}");
        }

        registrable.Name = command.Name;
        registrable.NameSecondary = command.NameSecondary;
        registrable.DisplayName = Enumerable.Empty<string?>()
                                            .Append(registrable.Name)
                                            .Append(registrable.NameSecondary)
                                            .StringJoinNullable(" - ");
        registrable.Tag = command.Tag;
        if (registrable.Type != command.Type)
        {
            if (@event.State != RegistrationForms.EventState.Setup)
            {
                throw new Exception($"To change the type of a track, event must be in state setup, but it is in state {@event.State}");
            }

            if (registrable.Spots?.Any() == true)
            {
                throw new Exception($"To change the type of a track, there must not be any registrations in it, but there are {registrable.Spots.Count}");
            }

            registrable.Type = command.Type;
        }

        switch (registrable.Type)
        {
            case RegistrableType.Single:
                registrable.MaximumSingleSeats = command.MaximumSingleSpots;
                registrable.MaximumDoubleSeats = null;
                registrable.MaximumAllowedImbalance = null;
                registrable.HasWaitingList = command.HasWaitingList && command.MaximumSingleSpots != null;
                break;

            case RegistrableType.Double:
                registrable.MaximumSingleSeats = null;
                registrable.MaximumDoubleSeats = command.MaximumDoubleSpots;
                registrable.MaximumAllowedImbalance = command.MaximumAllowedImbalance;
                registrable.HasWaitingList = command.HasWaitingList && command.MaximumDoubleSpots != null;
                break;
        }

        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         QueryName = nameof(RegistrablesOverviewQuery),
                                         EventId = command.EventId,
                                         DirtyMoment = _dateTimeProvider.Now
                                     });
        return Unit.Value;
    }
}