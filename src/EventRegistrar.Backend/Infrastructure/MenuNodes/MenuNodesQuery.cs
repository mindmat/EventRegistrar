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
}

public class MenuNodesQueryHandler(IQueryable<MenuNodeReadModel> nodes)
    : IRequestHandler<MenuNodesQuery, IEnumerable<MenuNodeContent>>
{
    public async Task<IEnumerable<MenuNodeContent>> Handle(MenuNodesQuery query, CancellationToken cancellationToken)
    {
        return await nodes.Where(mnd => mnd.EventId == query.EventId)
                          .Select(mnd => new MenuNodeContent
                                         {
                                             Key = mnd.Key,
                                             Content = mnd.Content,
                                             Style = mnd.Style,
                                             Hidden = mnd.Hidden
                                         })
                          .ToListAsync(cancellationToken);
    }
}