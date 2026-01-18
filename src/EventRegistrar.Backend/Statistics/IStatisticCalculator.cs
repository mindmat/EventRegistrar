namespace EventRegistrar.Backend.Statistics;

/// <summary>
/// Interface that all statistic calculations must implement.
/// The command will call all implementations, collect their results, and save them in the database.
/// </summary>
public interface IStatisticCalculator
{
    /// <summary>
    /// Unique name identifying this statistic calculation.
    /// This will be used as the key in the database.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Human-readable display name for this statistic.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Category to group related statistics together.
    /// </summary>
    string Category { get; }

    string? Unit { get { return null; } }


    /// <summary>
    /// Calculate the statistic values for multiple events at once.
    /// For event-bound statistics, provide the list of event IDs to process.
    /// For global statistics, pass an empty list - the implementation should ignore the eventIds parameter.
    /// </summary>
    /// <param name="eventIds">List of event IDs to calculate statistics for. Empty list for global statistics.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of event ID to calculated statistic data point. Null key for global statistics.</returns>
    Task<IDictionary<Guid, decimal>> CalculateBatch(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default);
}