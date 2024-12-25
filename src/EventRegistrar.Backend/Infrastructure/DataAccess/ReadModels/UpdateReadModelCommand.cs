using System.Text.Json;

using EventRegistrar.Backend.Infrastructure.MenuNodes;

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
                                           ChangeTrigger changeTrigger,
                                           RequestDateTimeProvider dateTimeProvider,
                                           IRepository<MenuNodeReadModel> menuNodes)
    : IRequestHandler<UpdateReadModelCommand>
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public async Task Handle(UpdateReadModelCommand command, CancellationToken cancellationToken)
    {
        var updater = calculators.First(rmu => rmu.QueryName == command.QueryName);

        var readModels = dbContext.Set<ReadModel>();
        var readModel = await readModels.AsTracking()
                                        .Where(rdm => rdm.QueryName == command.QueryName
                                                   && rdm.EventId == command.EventId
                                                   && rdm.RowId == command.RowId)
                                        .FirstOrDefaultAsync(cancellationToken);
        if (readModel?.LastUpdate >= command.DirtyMoment)
        {
            // Not perfect (time vs row version), but still servers as debouncing
            return;
        }

        var result = await updater.Calculate(command.EventId, command.RowId, cancellationToken);

        UpsertReadModel(command, result.ReadModel, readModel, readModels);

        if (result.MenuNode != null)
        {
            await UpsertMenuNode(command.EventId, result.MenuNode);
        }
    }

    private void UpsertReadModel(UpdateReadModelCommand command,
                                 object calculated,
                                 ReadModel? existing,
                                 DbSet<ReadModel> readModels)
    {
        var contentJson = JsonSerializer.Serialize(calculated, _serializerOptions);

        if (existing == null)
        {
            var node = new ReadModel
                       {
                           QueryName = command.QueryName,
                           EventId = command.EventId,
                           RowId = command.RowId,
                           ContentJson = contentJson,
                           LastUpdate = dateTimeProvider.RequestNow
                       };
            var entry = readModels.Attach(node);
            entry.State = EntityState.Added;
            changeTrigger.QueryChanged(command.QueryName,
                                       command.EventId,
                                       command.RowId);
        }
        else
        {
            existing.ContentJson = contentJson;
            var contentHasChanged = dbContext.Entry(existing).State == EntityState.Modified;
            if (contentHasChanged)
            {
                changeTrigger.QueryChanged(command.QueryName,
                                           command.EventId,
                                           command.RowId);
            }
            existing.LastUpdate = dateTimeProvider.RequestNow;
        }
    }

    private async Task UpsertMenuNode(Guid eventId, MenuNodeCalculation menuNodeCalculation)
    {
        var node = await menuNodes.AsTracking()
                                  .FirstOrDefaultAsync(mnr => mnr.EventId == eventId
                                                           && mnr.Key == menuNodeCalculation.Key);
        var anythingChanged = false;
        if (node == null)
        {
            anythingChanged = true;
            menuNodes.InsertObjectTree(new MenuNodeReadModel
                                       {
                                           Id = Guid.NewGuid(),
                                           EventId = eventId,
                                           Key = menuNodeCalculation.Key,
                                           Content = menuNodeCalculation.Content,
                                           Hidden = menuNodeCalculation.Hidden
                                       });
        }
        else if (node.Content != menuNodeCalculation.Content
              || node.Style != menuNodeCalculation.Style
              || node.Hidden != menuNodeCalculation.Hidden)
        {
            anythingChanged = true;
            node.Content = menuNodeCalculation.Content;
            node.Style = menuNodeCalculation.Style;
            node.Hidden = menuNodeCalculation.Hidden;
        }

        if (anythingChanged)
        {
            changeTrigger.QueryChanged<MenuNodesQuery>(eventId);
        }
    }
}

public interface IReadModelCalculator
{
    string QueryName { get; }
    bool IsDateDependent { get; }
    Task<(object ReadModel, MenuNodeCalculation? MenuNode)> Calculate(Guid eventId, Guid? rowId, CancellationToken cancellationToken);
}

public abstract class ReadModelCalculator<T> : IReadModelCalculator
    where T : class
{
    public abstract string QueryName { get; }
    public abstract bool IsDateDependent { get; }

    public async Task<(object ReadModel, MenuNodeCalculation? MenuNode)> Calculate(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        return await CalculateTyped(eventId, rowId, cancellationToken);
    }

    protected abstract Task<(T ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken);
}

public class MenuNodeCalculation
{
    public required MenuNodeKey Key { get; set; }
    public string? Content { get; set; }
    public MenuNodeStyle? Style { get; set; }
    public bool Hidden { get; set; }
}