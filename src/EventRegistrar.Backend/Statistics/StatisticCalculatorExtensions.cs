namespace EventRegistrar.Backend.Statistics;

/// <summary>
/// Extension methods for working with statistic calculators and their metadata.
/// </summary>
public static class StatisticCalculatorExtensions
{
    /// <summary>
    /// Creates a lookup dictionary for quick access to calculator metadata by statistic name.
    /// </summary>
    public static Dictionary<string, IStatisticCalculator> ToLookup(this IEnumerable<IStatisticCalculator> calculators)
    {
        return calculators.ToDictionary(c => c.Key);
    }
}