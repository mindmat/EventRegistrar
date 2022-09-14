using System.Text.Json;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class UpdateReadModelCommand : IRequest
{
    public string QueryName { get; set; } = null!;
    public Guid EventId { get; set; }
    public Guid? RowId { get; set; }
    public DateTimeOffset DirtyMoment { get; set; }
}

public class UpdateReadModelCommandHandler : IRequestHandler<UpdateReadModelCommand>
{
    private readonly IEnumerable<IReadModelUpdater> _updaters;
    private readonly DbContext _dbContext;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public UpdateReadModelCommandHandler(IEnumerable<IReadModelUpdater> updaters,
                                         DbContext dbContext,
                                         IEventBus eventBus,
                                         IDateTimeProvider dateTimeProvider)
    {
        _updaters = updaters;
        _dbContext = dbContext;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateReadModelCommand command, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.Now;
        var updater = _updaters.First(rmu => rmu.QueryName == command.QueryName);

        var readModels = _dbContext.Set<ReadModel>();

        var readModel = await readModels.AsTracking()
                                        .Where(rdm => rdm.QueryName == command.QueryName
                                                   && rdm.EventId == command.EventId
                                                   && rdm.RowId == command.RowId)
                                        .FirstOrDefaultAsync(cancellationToken);
        if (readModel?.LastUpdate >= command.DirtyMoment)
        {
            // Not perfect (time vs rowversion)
            return Unit.Value;
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
            _eventBus.Publish(new ReadModelUpdated
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
                _eventBus.Publish(new ReadModelUpdated
                                  {
                                      QueryName = command.QueryName,
                                      EventId = command.EventId,
                                      RowId = command.RowId
                                  });
            }
        }

        return Unit.Value;
    }
}

public interface IReadModelUpdater
{
    string QueryName { get; }
    bool IsDateDependent { get; }
    Task<object> Calculate(Guid eventId, Guid? rowId, CancellationToken cancellationToken);
}

public abstract class ReadModelUpdater<T> : IReadModelUpdater
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