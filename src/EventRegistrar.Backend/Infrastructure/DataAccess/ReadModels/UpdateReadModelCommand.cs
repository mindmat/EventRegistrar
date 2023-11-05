﻿using System.Text.Json;

using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Register;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ChangeTrigger
{
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly EventContext _eventContext;
    private readonly IEnumerable<IReadModelCalculator> _calculators;
    private readonly IEventBus _eventBus;

    public ChangeTrigger(CommandQueue commandQueue,
                         IDateTimeProvider dateTimeProvider,
                         EventContext eventContext,
                         IEnumerable<IReadModelCalculator> calculators,
                         IEventBus eventBus)
    {
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _eventContext = eventContext;
        _calculators = calculators;
        _eventBus = eventBus;
    }

    public void TriggerUpdate<T>(Guid? rowId = null, Guid? eventId = null)
        where T : IReadModelCalculator
    {
        eventId ??= _eventContext.EventId;
        if (eventId == null)
        {
            throw new ArgumentNullException(nameof(eventId));
        }

        var queryName = _calculators.First(cal => cal.GetType() == typeof(T))
                                    .QueryName;

        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         QueryName = queryName,
                                         EventId = eventId.Value,
                                         RowId = rowId,
                                         DirtyMoment = _dateTimeProvider.Now
                                     });
    }

    public void QueryChanged<TQuery>(Guid eventId, Guid? rowId = null)
        where TQuery : IEventBoundRequest
    {
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = eventId,
                              QueryName = typeof(TQuery).Name,
                              RowId = rowId
                          });
    }

    public void PublishEvent<TEvent>(TEvent @event)
        where TEvent : DomainEvent
    {
        _eventBus.Publish(@event);
    }

    public void EnqueueCommand<T>(T command)
        where T : IRequest
    {
        _commandQueue.EnqueueCommand(command);
    }
}

public class UpdateReadModelCommand : IRequest
{
    public Guid EventId { get; set; }
    public string QueryName { get; set; } = null!;
    public Guid? RowId { get; set; }
    public DateTimeOffset DirtyMoment { get; set; }
}

public class UpdateReadModelCommandHandler : AsyncRequestHandler<UpdateReadModelCommand>
{
    private readonly IEnumerable<IReadModelCalculator> _calculators;
    private readonly DbContext _dbContext;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public UpdateReadModelCommandHandler(IEnumerable<IReadModelCalculator> calculators,
                                         DbContext dbContext,
                                         IEventBus eventBus,
                                         IDateTimeProvider dateTimeProvider)
    {
        _calculators = calculators;
        _dbContext = dbContext;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task Handle(UpdateReadModelCommand command, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.Now;
        var updater = _calculators.First(rmu => rmu.QueryName == command.QueryName);

        var readModels = _dbContext.Set<ReadModel>();

        var readModel = await readModels.AsTracking()
                                        .Where(rdm => rdm.QueryName == command.QueryName
                                                   && rdm.EventId == command.EventId
                                                   && rdm.RowId == command.RowId)
                                        .FirstOrDefaultAsync(cancellationToken);
        if (readModel?.LastUpdate >= command.DirtyMoment)
        {
            // Not perfect (time vs row version)
            return;
        }

        var result = await updater.Calculate(command.EventId, command.RowId, cancellationToken);

        var contentJson = JsonSerializer.Serialize(result, _serializerOptions);

        if (readModel == null)
        {
            readModel = new ReadModel
                        {
                            QueryName = command.QueryName,
                            EventId = command.EventId,
                            RowId = command.RowId,
                            ContentJson = contentJson,
                            LastUpdate = now
                        };
            var entry = readModels.Attach(readModel);
            entry.State = EntityState.Added;
            _eventBus.Publish(new QueryChanged
                              {
                                  QueryName = command.QueryName,
                                  EventId = command.EventId,
                                  RowId = command.RowId
                              });
        }
        else
        {
            readModel.ContentJson = contentJson;
            if (_dbContext.Entry(readModel).State == EntityState.Modified)
            {
                _eventBus.Publish(new QueryChanged
                                  {
                                      QueryName = command.QueryName,
                                      EventId = command.EventId,
                                      RowId = command.RowId
                                  });
            }
        }
    }
}

public interface IReadModelCalculator
{
    string QueryName { get; }
    bool IsDateDependent { get; }
    Task<object> Calculate(Guid eventId, Guid? rowId, CancellationToken cancellationToken);
}

public abstract class ReadModelCalculator<T> : IReadModelCalculator
    where T : class
{
    public abstract string QueryName { get; }
    public abstract bool IsDateDependent { get; }

    public async Task<object> Calculate(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        return await CalculateTyped(eventId, rowId, cancellationToken);
    }

    public abstract Task<T> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken);
}