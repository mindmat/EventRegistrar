namespace EventRegistrar.Backend.Registrables.Tags;

public class RegistrableTagsQuery : IRequest<IEnumerable<RegistrableTagDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrableTagsQueryHandler(IQueryable<RegistrableTag> tags) : IRequestHandler<RegistrableTagsQuery, IEnumerable<RegistrableTagDisplayItem>>
{
    public async Task<IEnumerable<RegistrableTagDisplayItem>> Handle(RegistrableTagsQuery query,
                                                                     CancellationToken cancellationToken)
    {
        var ordered = await tags.Where(rbl => rbl.EventId == query.EventId)
                                .OrderBy(rbt => rbt.SortKey)
                                .ThenBy(rbt => rbt.Tag)
                                .Select(rbt => new RegistrableTagDisplayItem
                                               {
                                                   TagId = rbt.Id,
                                                   Tag = rbt.Tag,
                                                   Text = rbt.FallbackText,
                                                   Color = rbt.Color,
                                                   SortKey = rbt.SortKey
                                               })
                                .ToListAsync(cancellationToken);

        for (var i = 0; i < ordered.Count && i < RegistrableTagDefaults.Colors.Length; i++)
        {
            ordered[i].Color ??= RegistrableTagDefaults.Colors[i];
        }

        return ordered;
    }
}

public record RegistrableTagDisplayItem
{
    public Guid TagId { get; set; }
    public string Text { get; set; } = null!;
    public string Tag { get; set; } = null!;
    public string? Color { get; set; }
    public int SortKey { get; set; }
}