using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class RemarksOverviewCalculator(IQueryable<Registration> registrations) : ReadModelCalculator<IEnumerable<RemarksDisplayItem>>
{
    public override string QueryName => nameof(RemarksOverviewQuery);
    public override bool IsDateDependent => false;

    protected override async Task<(IEnumerable<RemarksDisplayItem> ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var remarks = await registrations.Where(reg => reg.EventId == eventId
                                                    && reg.Remarks != null
                                                    && reg.Remarks != string.Empty)
                                         .OrderByDescending(reg => reg.ReceivedAt)
                                         .Select(reg => new RemarksDisplayItem
                                                        {
                                                            RegistrationId = reg.Id,
                                                            DisplayName = $"{reg.RespondentFirstName} {reg.RespondentLastName}",
                                                            Email = reg.RespondentEmail,
                                                            Remarks = reg.Remarks!,
                                                            Processed = reg.RemarksProcessed
                                                        })
                                         .ToListAsync(cancellationToken);
        return (remarks, CalculateNode(remarks));
    }

    private MenuNodeCalculation? CalculateNode(List<RemarksDisplayItem> remarks)
    {
        var node = new MenuNodeCalculation
                   {
                       Key = MenuNodeKey.Remarks
                   };
        var unprocessedCount = remarks.Count(reg => !reg.Processed);
        if (unprocessedCount > 0)
        {
            node.Content = $"{unprocessedCount}";
            node.Style = MenuNodeStyle.ToDo;
        }

        return node;
    }
}

public class RemarksDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string Remarks { get; set; } = null!;
    public bool Processed { get; set; }
}