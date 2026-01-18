namespace EventRegistrar.Backend.Statistics;

/// <summary>
/// Query to retrieve stored statistic snapshots with metadata from calculators.
/// </summary>
public class StatisticsQuery : IRequest<IEnumerable<StatisticSnapshotDisplayItem>>
{
    /// <summary>
    /// Optional event ID to filter statistics.
    /// If null, returns global statistics and optionally event-specific ones.
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    /// Optional category filter (matched against calculator metadata).
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Optional statistic name filter.
    /// </summary>
    public string? StatisticName { get; set; }

    /// <summary>
    /// Start date for time range filtering.
    /// </summary>
    public DateTimeOffset? FromDate { get; set; }

    /// <summary>
    /// End date for time range filtering.
    /// </summary>
    public DateTimeOffset? ToDate { get; set; }

    /// <summary>
    /// Whether to include global (non-event-bound) statistics.
    /// Default: true.
    /// </summary>
    public bool IncludeGlobal { get; set; } = true;

    /// <summary>
    /// Maximum number of results to return.
    /// Default: 1000.
    /// </summary>
    public int MaxResults { get; set; } = 1000;
}

public class StatisticsQueryHandler(IQueryable<StatisticSnapshot> statisticSnapshots,
                                    IEnumerable<IStatisticCalculator> statisticCalculators)
    : IRequestHandler<StatisticsQuery, IEnumerable<StatisticSnapshotDisplayItem>>
{
    public async Task<IEnumerable<StatisticSnapshotDisplayItem>> Handle(StatisticsQuery query, CancellationToken cancellationToken)
    {
        // Create a lookup for calculator metadata
        var calculatorLookup = statisticCalculators.ToLookup();

        var queryable = statisticSnapshots.AsQueryable();

        // Apply filters
        if (query.EventId.HasValue)
        {
            if (query.IncludeGlobal)
            {
                queryable = queryable.Where(ss => ss.EventId == query.EventId.Value || ss.EventId == null);
            }
            else
            {
                queryable = queryable.Where(ss => ss.EventId == query.EventId.Value);
            }
        }
        else if (!query.IncludeGlobal)
        {
            queryable = queryable.Where(ss => ss.EventId != null);
        }

        if (!string.IsNullOrEmpty(query.StatisticName))
        {
            queryable = queryable.Where(ss => ss.Key == query.StatisticName);
        }

        if (query.FromDate.HasValue)
        {
            queryable = queryable.Where(ss => ss.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            queryable = queryable.Where(ss => ss.CreatedAt <= query.ToDate.Value);
        }

        var snapshots = await queryable.OrderByDescending(ss => ss.CreatedAt)
                                       .Take(query.MaxResults)
                                       .Select(sts => new
                                                      {
                                                          sts.Id,
                                                          StatisticName = sts.Key,
                                                          sts.EventId,
                                                          EventName = sts.Event != null ? sts.Event.Name : null,
                                                          sts.CreatedAt,
                                                          sts.Value
                                                      })
                                       .ToListAsync(cancellationToken);

        var results = snapshots.Select(sts =>
                               {
                                   // Get metadata from calculator
                                   calculatorLookup.TryGetValue(sts.StatisticName, out var calculator);

                                   return new StatisticSnapshotDisplayItem
                                          {
                                              Id = sts.Id,
                                              StatisticName = sts.StatisticName,
                                              DisplayName = calculator?.DisplayName ?? sts.StatisticName,
                                              Category = calculator?.Category ?? "Unknown",
                                              EventId = sts.EventId,
                                              EventName = sts.EventName,
                                              CreatedAt = sts.CreatedAt,
                                              Value = sts.Value,
                                              Unit = calculator?.Unit
                                          };
                               })
                               .ToList();

        // Apply category filter if specified (now that we have metadata from calculators)
        if (!string.IsNullOrEmpty(query.Category))
        {
            results = results.Where(r => r.Category == query.Category).ToList();
        }

        return results;
    }
}

public class StatisticSnapshotDisplayItem
{
    public Guid Id { get; set; }
    public string StatisticName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public Guid? EventId { get; set; }
    public string? EventName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public decimal Value { get; set; }
    public string? Unit { get; set; }
}