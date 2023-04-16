namespace EventRegistrar.Backend.Registrables.Tags;

public class RegistrableTagsQuery : IRequest<IEnumerable<RegistrableTagDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrableTagsQueryHandler : IRequestHandler<RegistrableTagsQuery, IEnumerable<RegistrableTagDisplayItem>>
{
    private readonly IQueryable<RegistrableTag> _tags;

    public RegistrableTagsQueryHandler(IQueryable<RegistrableTag> tags)
    {
        _tags = tags;
    }

    public async Task<IEnumerable<RegistrableTagDisplayItem>> Handle(RegistrableTagsQuery query,
                                                                     CancellationToken cancellationToken)
    {
        return await _tags.Where(rbl => rbl.EventId == query.EventId)
                          .OrderBy(rbt => rbt.SortKey)
                          .ThenBy(rbt => rbt.Tag)
                          .Select(rbt => new RegistrableTagDisplayItem
                                         {
                                             TagId = rbt.Id,
                                             Tag = rbt.Tag,
                                             Text = rbt.FallbackText,
                                             SortKey = rbt.SortKey
                                         })
                          .ToListAsync(cancellationToken);
    }
}

public record RegistrableTagDisplayItem
{
    public Guid TagId { get; set; }
    public string Text { get; set; } = null!;
    public string Tag { get; set; } = null!;
    public int SortKey { get; set; }
}