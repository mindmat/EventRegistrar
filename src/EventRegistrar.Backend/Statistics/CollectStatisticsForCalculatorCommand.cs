using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Statistics;

/// <summary>
/// Command to collect statistics for a specific calculator across all active events.
/// This is triggered from the nightly jobs to distribute processing across multiple queue messages.
/// </summary>
public class CollectStatisticsForCalculatorCommand : IRequest
{
    /// <summary>
    /// Name of the specific calculator to run.
    /// </summary>
    public required string CalculatorKey { get; set; }

    public IEnumerable<Guid> EventIds { get; set; } = [];
}

public class CollectStatisticsForCalculatorCommandHandler(IEnumerable<IStatisticCalculator> statisticCalculators,
                                                          IRepository<StatisticSnapshot> statisticSnapshots,
                                                          RequestDateTimeProvider dateTimeProvider,
                                                          ILogger<CollectStatisticsForCalculatorCommandHandler> logger)
    : IRequestHandler<CollectStatisticsForCalculatorCommand>
{
    public async Task Handle(CollectStatisticsForCalculatorCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting statistics collection for calculator: {CalculatorKey}", command.CalculatorKey);

        // Find the specific calculator
        var calculator = statisticCalculators.FirstOrDefault(c => c.Key == command.CalculatorKey);
        if (calculator == null)
        {
            logger.LogWarning("Calculator {CalculatorKey} not found", command.CalculatorKey);
            return;
        }

        // Calculate statistics for all events (no filtering based on recent snapshots)
        var valuesPerEvent = await calculator.CalculateBatch(command.EventIds, cancellationToken);
        foreach (var value in valuesPerEvent)
        {
            statisticSnapshots.InsertObjectTree(new StatisticSnapshot
                                                {
                                                    Id = Guid.NewGuid(),
                                                    Key = calculator.Key,
                                                    EventId = value.Key,
                                                    CreatedAt = dateTimeProvider.RequestNow,
                                                    Value = value.Value
                                                });
        }

        logger.LogInformation("Statistics collection completed for {EventCount} events for {CalculatorKey}.", valuesPerEvent.Count, calculator.Key);
    }
}