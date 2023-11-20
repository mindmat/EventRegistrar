namespace EventRegistrar.Backend.Infrastructure.ReadableIds;

public class ReadableIdProvider(IRepository<ReadableIdSource> sources)
{
    public async Task<string> GetNextId(Guid eventId, string? prefix)
    {
        var source = await sources.AsTracking()
                                  .FirstOrDefaultAsync(ris => ris.EventId == eventId
                                                           && ris.Prefix == prefix)
                  ?? sources.InsertObjectTree(new ReadableIdSource
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