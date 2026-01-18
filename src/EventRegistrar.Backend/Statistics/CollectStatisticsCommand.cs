using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Statistics;

/// <summary>
/// Command to collect statistics from all calculators.
/// This command fetches all statistic calculators and triggers individual collection commands for each.
/// </summary>
public class CollectStatisticsCommand : IRequest;

public class CollectStatisticsCommandHandler(IEnumerable<IStatisticCalculator> statisticCalculators,
                                             IQueryable<Event> events,
                                             ChangeTrigger changeTrigger)
    : IRequestHandler<CollectStatisticsCommand>
{
    public async Task Handle(CollectStatisticsCommand command, CancellationToken cancellationToken)
    {
        var eventIds_Active = await GetActiveEventIds(cancellationToken);
        if (eventIds_Active.Count == 0)
        {
            return;
        }

        foreach (var calculator in statisticCalculators)
        {
            changeTrigger.EnqueueCommand(new CollectStatisticsForCalculatorCommand
                                         {
                                             CalculatorKey = calculator.Key,
                                             EventIds = eventIds_Active
                                         });
        }
    }

    private async Task<List<Guid>> GetActiveEventIds(CancellationToken cancellationToken)
    {
        return await events.Where(evt => evt.State != RegistrationForms.EventState.Setup
                                      && evt.State != RegistrationForms.EventState.Finished)
                           .Select(evt => evt.Id)
                           .ToListAsync(cancellationToken);
    }
}