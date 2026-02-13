using System.Text.Json;
using System.Text.RegularExpressions;

using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Infrastructure.MenuNodes;

public class MenuNodesQuery : IRequest<IEnumerable<MenuNodeContent>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class MenuNodeContent
{
    public MenuNodeKey Key { get; set; }
    public string? Content { get; set; }
    public MenuNodeStyle? Style { get; set; }
    public bool Hidden { get; set; }
    public string? ContentToolTip { get; set; }
}

public partial class MenuNodesQueryHandler(IQueryable<MenuNodeReadModel> nodes)
    : IRequestHandler<MenuNodesQuery, IEnumerable<MenuNodeContent>>
{
    public async Task<IEnumerable<MenuNodeContent>> Handle(MenuNodesQuery query, CancellationToken cancellationToken)
    {
        var nodeData = await nodes.Where(mnd => mnd.EventId == query.EventId)
                                  .Select(mnd => new
                                                 {
                                                     mnd.Key,
                                                     mnd.Content,
                                                     mnd.Style,
                                                     mnd.Hidden,
                                                     mnd.ToolTipData,
                                                     mnd.ToolTipDataType
                                                 })
                                  .ToListAsync(cancellationToken);

        return nodeData.Select(mnd => new MenuNodeContent
                                      {
                                          Key = mnd.Key,
                                          Content = mnd.Content,
                                          Style = mnd.Style,
                                          Hidden = mnd.Hidden,
                                          ContentToolTip = GenerateToolTip(mnd.Key, mnd.ToolTipData, mnd.ToolTipDataType)
                                      })
                       .ToList();
    }

    private static string? GenerateToolTip(MenuNodeKey key, string? toolTipData, string? toolTipDataType)
    {
        if (toolTipData == null || toolTipDataType == null)
        {
            return null;
        }

        var resourceKey = $"{key}_Tooltip";
        var template = Resources.ResourceManager.GetString(resourceKey);
        if (template == null)
        {
            return null;
        }

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(toolTipData);
            return PlaceholderRegex()
                .Replace(template, match =>
                {
                    var placeholder = match.Groups[1].Value;
                    return data.TryGetProperty(placeholder, out var value)
                               ? value.ToString()
                               : match.Value;
                });
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderRegex();
}