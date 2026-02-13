using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class RemarksOverviewCalculator(IQueryable<RegistrationRemark> registrationRemarks)
    : ReadModelCalculator<IEnumerable<RemarksDisplayItem>>
{
    public override string QueryName => nameof(RemarksOverviewQuery);
    public override bool IsDateDependent => false;

    protected override async Task<(IEnumerable<RemarksDisplayItem> ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var remarks = await registrationRemarks.Where(rmk => rmk.Registration!.EventId == eventId
                                                          && rmk.Registration.State != RegistrationState.Cancelled)
                                               .OrderByDescending(rmk => rmk.Registration!.ReceivedAt)
                                               .Select(rmk => new RemarksDisplayItem
                                                              {
                                                                  Id = rmk.Id,
                                                                  RegistrationId = rmk.RegistrationId,
                                                                  DisplayName = $"{rmk.Registration!.RespondentFirstName} {rmk.Registration.RespondentLastName}",
                                                                  Email = rmk.Registration.RespondentEmail,
                                                                  Section = rmk.Question!.Section,
                                                                  Remarks = rmk.Text,
                                                                  Processed = rmk.Processed
                                                              })
                                               .ToListAsync(cancellationToken);
        return (remarks, CalculateNode(remarks));
    }

    private MenuNodeCalculation CalculateNode(List<RemarksDisplayItem> remarks)
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
            node.ToolTipData = new RemarksToolTipData { UnprocessedCount = unprocessedCount };
        }

        return node;
    }
}

public class RemarksToolTipData
{
    public int UnprocessedCount { get; set; }
}

public class RemarksDisplayItem
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Section { get; set; }
    public string Remarks { get; set; } = null!;
    public bool Processed { get; set; }
}