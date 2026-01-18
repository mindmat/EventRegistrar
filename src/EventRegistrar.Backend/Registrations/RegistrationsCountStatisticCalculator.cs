using EventRegistrar.Backend.Statistics;

namespace EventRegistrar.Backend.Registrations;

/// <summary>
/// Calculates the total number of registrations per event.
/// </summary>
public class RegistrationsCountStatisticCalculator(IQueryable<Registration> registrations)
    : IStatisticCalculator
{
    public string Key => "RegistrationsCount";
    public string DisplayName => "Total Registrations";
    public string Category => "Registrations";

    /// <summary>
    /// Calculate registration count statistics for multiple events at once.
    /// </summary>
    public async Task<IDictionary<Guid, decimal>> CalculateBatch(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
    {
        return await registrations.Where(reg => eventIds.Contains(reg.EventId)
                                             && reg.State != RegistrationState.Cancelled)
                                  .GroupBy(reg => reg.EventId)
                                  .ToDictionaryAsync(grp => grp.Key,
                                                     grp => (decimal)grp.Count(),
                                                     cancellationToken);
    }
}