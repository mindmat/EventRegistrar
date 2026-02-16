using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;
using EventRegistrar.Backend.Registrations.Raw;

namespace EventRegistrar.Backend.Registrations.Register;

public class ProcessingErrorsCalculator(IQueryable<RawRegistration> rawRegistrations, IQueryable<Event> events) : ReadModelCalculator<IEnumerable<ProcessingError>>
{
    public override string QueryName => nameof(ProcessingErrorsQuery);
    public override bool IsDateDependent => false;

    protected override async Task<(IEnumerable<ProcessingError> ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var eventAcronym = await events.Where(evt => evt.Id == eventId)
                                       .Select(evt => evt.Acronym)
                                       .FirstAsync(cancellationToken);

        var errors = await rawRegistrations.Where(rrg => rrg.EventAcronym == eventAcronym
                                                      && rrg.Processed == null
                                                      && rrg.LastProcessingError != null)
                                           .Select(rrg => new ProcessingError
                                                          {
                                                              RawRegistrationId = rrg.Id,
                                                              Created = rrg.Created,
                                                              LastProcessingError = rrg.LastProcessingError,
                                                              FirstName = rrg.FirstName,
                                                              LastName = rrg.LastName,
                                                              Mail = rrg.Mail,
                                                              Phone = rrg.Phone,
                                                              RoleMissing = rrg.RoleMissing,
                                                              RoleOverride = rrg.RoleOverride
                                                          })
                                           .OrderByDescending(rrg => rrg.Created)
                                           .ToListAsync(cancellationToken);

        var node = new MenuNodeCalculation
                   {
                       Key = MenuNodeKey.FixRawProcessing
                   };

        if (errors.Count > 0)
        {
            node.Content = $"{errors.Count}";
            node.Style = MenuNodeStyle.Important;
            node.ToolTipData = new FixRawProcessingToolTipData { ErrorCount = errors.Count };
        }
        else
        {
            node.Hidden = true;
        }

        return (errors, node);
    }
}

public class FixRawProcessingToolTipData
{
    public int ErrorCount { get; set; }
}

public class ProcessingError
{
    public Guid RawRegistrationId { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? LastProcessingError { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Mail { get; set; }
    public string? Phone { get; set; }
    public bool RoleMissing { get; set; }
    public Role? RoleOverride { get; set; }
}