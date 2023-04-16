namespace EventRegistrar.Backend.Infrastructure.ReadableIds;

public class ReadableIdProvider
{
    private readonly IRepository<ReadableIdSource> _sources;

    public ReadableIdProvider(IRepository<ReadableIdSource> sources)
    {
        _sources = sources;
    }

    public async Task<string> GetNextId(Guid eventId, string? prefix)
    {
        var source = await _sources.AsTracking()
                                   .FirstOrDefaultAsync(ris => ris.EventId == eventId
                                                            && ris.Prefix == prefix)
                  ?? _sources.InsertObjectTree(new ReadableIdSource
                                               {
                                                   Id = Guid.NewGuid(),
                                                   EventId = eventId,
                                                   Prefix = prefix,
                                                   Next = 1
                                               });
        var readableId = $"{prefix}{source.Next}";
        source.Next += 1;
        return readableId;
    }
}