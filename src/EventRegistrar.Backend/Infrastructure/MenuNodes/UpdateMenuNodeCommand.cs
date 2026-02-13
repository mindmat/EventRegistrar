using System.Text.Json;

using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Infrastructure.MenuNodes;

public class UpdateMenuNodeCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public MenuNodeKey Key { get; set; }
}

public class UpdateMenuNodeCommandHandler(IRepository<MenuNodeReadModel> nodes,
                                          IEnumerable<IMenuNodeCalculator> calculators,
                                          ChangeTrigger changeTrigger)
    : IRequestHandler<UpdateMenuNodeCommand>
{
    public async Task Handle(UpdateMenuNodeCommand command, CancellationToken cancellationToken)
    {
        var calculator = calculators.FirstOrDefault(calc => calc.Key == command.Key);
        if (calculator == null)
        {
            return;
        }

        var calculation = await calculator.Calculate(command.EventId, cancellationToken);
        var toolTipData = calculation.ToolTipData != null
                              ? JsonSerializer.Serialize(calculation.ToolTipData)
                              : null;
        var toolTipDataType = calculation.ToolTipData?.GetType().FullName;

        var node = await nodes.AsTracking()
                              .FirstOrDefaultAsync(mnr => mnr.EventId == command.EventId
                                                       && mnr.Key == calculator.Key,
                                                   cancellationToken);
        var anythingChanged = false;
        if (node == null)
        {
            anythingChanged = true;
            nodes.InsertObjectTree(new MenuNodeReadModel
            {
                Id = Guid.NewGuid(),
                EventId = command.EventId,
                Key = command.Key,
                Content = calculation.Content,
                Hidden = calculation.Hidden,
                ToolTipData = toolTipData,
                ToolTipDataType = toolTipDataType
            });
        }
        else if (node.Content != calculation.Content
              || node.Style != calculation.Style
              || node.Hidden != calculation.Hidden
              || node.ToolTipData != toolTipData
              || node.ToolTipDataType != toolTipDataType)
        {
            anythingChanged = true;
            node.Content = calculation.Content;
            node.Style = calculation.Style;
            node.Hidden = calculation.Hidden;
            node.ToolTipData = toolTipData;
            node.ToolTipDataType = toolTipDataType;
        }

        if (anythingChanged)
        {
            changeTrigger.QueryChanged<MenuNodesQuery>(command.EventId);
        }
    }
}

public interface IMenuNodeCalculator
{
    MenuNodeKey Key { get; }
    Task<MenuNodeCalculation> Calculate(Guid eventId, CancellationToken cancellationToken);
}


public class MenuNodeCalculation
{
    public required MenuNodeKey Key { get; set; }
    public string? Content { get; set; }
    public MenuNodeStyle? Style { get; set; }
    public bool Hidden { get; set; }
    public object? ToolTipData { get; set; }
}