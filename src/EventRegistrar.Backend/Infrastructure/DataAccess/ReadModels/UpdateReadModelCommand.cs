using System.Text.Json;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class UpdateReadModelCommand : IRequest
{
    public Guid EventId { get; set; }
    public string QueryName { get; set; } = null!;
    public Guid? RowId { get; set; }
    public DateTimeOffset? DirtyMoment { get; set; }
}

public class UpdateReadModelCommandHandler(IEnumerable<IReadModelCalculator> calculators,
                                           DbContext dbContext,
                                           IEventBus eventBus,
                                           IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateReadModelCommand>
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public async Task Handle(UpdateReadModelCommand command, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;
        var updater = calculators.First(rmu => rmu.QueryName == command.QueryName);

        var readModels = dbContext.Set<ReadModel>();

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
            eventBus.Publish(new QueryChanged
                             {
                                 QueryName = command.QueryName,
                                 EventId = command.EventId,
                                 RowId = command.RowId
                             });
        }
        else
        {
            readModel.ContentJson = contentJson;
            if (dbContext.Entry(readModel).State == EntityState.Modified)
            {
                eventBus.Publish(new QueryChanged
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