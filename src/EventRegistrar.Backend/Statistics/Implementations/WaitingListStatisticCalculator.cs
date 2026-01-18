using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Statistics.Implementations;

/// <summary>
/// Calculates the waiting list statistics per event.
/// </summary>
public class WaitingListStatisticCalculator(IQueryable<Registration> registrations)
    : IStatisticCalculator
{
    public string Key => "RegistrationsOnWaitingListCount";
    public string DisplayName => "Waiting List Count";
    public string Category => "Registrations";

    /// <summary>
    /// Calculate waiting list statistics for multiple events at once.
    /// </summary>
    public async Task<IDictionary<Guid, decimal>> CalculateBatch(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
    {
        // Single query for all events - much more efficient than individual queries
        return await registrations.Where(reg => eventIds.Contains(reg.EventId))
                                  .Where(reg => reg.State != RegistrationState.Cancelled
                                             && reg.IsOnWaitingList == true)
                                  .GroupBy(reg => reg.EventId)
                                  .ToDictionaryAsync(grp => grp.Key,
                                                     grp => (decimal)grp.Count(),
                                                     cancellationToken);
    }
}