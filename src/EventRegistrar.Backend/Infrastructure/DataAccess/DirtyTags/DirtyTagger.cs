using System.Data;

using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;

public class DirtyTagger
{
    private readonly CommandQueue _commandQueue;
    private readonly IEnumerable<IDirtySegment> _dirtySegments;
    private readonly DbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DirtyTagger(CommandQueue commandQueue,
                       IEnumerable<IDirtySegment> dirtySegments,
                       DbContext dbContext,
                       IDateTimeProvider dateTimeProvider)
    {
        _commandQueue = commandQueue;
        _dirtySegments = dirtySegments;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public void UpdateSegment<TSegment>(Guid entityId)
        where TSegment : IDirtySegment
    {
        var dirtySegment = _dirtySegments.First(dys => dys.GetType() == typeof(TSegment));
        dirtySegment.EnqueueCommand(_commandQueue, entityId);
        var dirtyTags = _dbContext.Set<DirtyTag>();
        dirtyTags.Add(new DirtyTag
                      {
                          Entity = dirtySegment.Entity,
                          EntityId = entityId,
                          Segment = dirtySegment.Name,
                          DirtyMoment = _dateTimeProvider.Now
                      });
    }

    public async Task<IEnumerable<DirtyTag>> IsDirty<TSegment>(Guid entityId)
        where TSegment : IDirtySegment
    {
        var dirtySegment = _dirtySegments.First(dys => dys.GetType() == typeof(TSegment));
        return await _dbContext.Set<DirtyTag>()
                               .Where(dyt => dyt.Entity == dirtySegment.Entity
                                          && dyt.EntityId == entityId
                                          && dyt.Segment == dirtySegment.Name)
                               .ToListAsync();
    }

    public void RemoveDirtyTags(IEnumerable<DirtyTag> dirtyTags)
    {
        _dbContext.Set<DirtyTag>()
                  .RemoveRange(dirtyTags);
    }

    public async Task WaitForRemovedTags(Guid entityId, params Type[] segmentNames)
    {
        if (!segmentNames.Any())
        {
            return;
        }

        var now = _dateTimeProvider.Now;
        var tagSet = _dbContext.Set<DirtyTag>();
        var segments = _dirtySegments.Where(dys => segmentNames.Contains(dys.GetType()))
                                     .ToList();

        var watchdog = 10;
        do
        {
            var tags = await tagSet.Where(dyt => dyt.EntityId == entityId
                                              && dyt.DirtyMoment < now)
                                   .ToListAsync();
            if (!tags.Any())
            {
                return;
            }

            var dirty = segments.Any(segment => tags.Any(tag => tag.Entity == segment.Entity
                                                             && tag.Segment == segment.Name));
            if (!dirty)
            {
                return;
            }

            await Task.Delay(2 * 1000);
        } while (--watchdog > 0);

        throw new DataException("Dirty tags were not removed in time");
    }
}

public interface IDirtySegment
{
    public void EnqueueCommand(CommandQueue commandQueue, Guid entityId);
    public string Entity { get; }
    public string Name { get; }
}