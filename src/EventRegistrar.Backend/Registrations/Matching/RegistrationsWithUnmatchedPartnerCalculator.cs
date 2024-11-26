using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;

namespace EventRegistrar.Backend.Registrations.Matching;

public class RegistrationsWithUnmatchedPartnerCalculator(IQueryable<Registration> registrations) : ReadModelCalculator<IEnumerable<PotentialPartnerMatch>>
{
    public override string QueryName => nameof(RegistrationsWithUnmatchedPartnerQuery);
    public override bool IsDateDependent => false;
    protected override async Task<(IEnumerable<PotentialPartnerMatch> ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var unmatched= await registrations.Where(reg => reg.EventId == eventId
                                             && reg.State != RegistrationState.Cancelled
                                             && reg.PartnerNormalized != null
                                             && reg.RegistrationId_Partner == null)
                                  .OrderByDescending(reg => reg.ReceivedAt)
                                  .Select(reg => new PotentialPartnerMatch
                                                 {
                                                     RegistrationId = reg.Id,
                                                     Email = reg.RespondentEmail,
                                                     FirstName = reg.RespondentFirstName,
                                                     LastName = reg.RespondentLastName,
                                                     State = reg.State.ToString(),
                                                     DeclaredPartner = reg.PartnerOriginal,
                                                     IsOnWaitingList = reg.IsOnWaitingList == true,
                                                     Tracks = reg.Seats_AsLeader!
                                                                 .Select(spt => new TrackMatch { Name = spt.Registrable!.DisplayName })
                                                                 .Union(reg.Seats_AsFollower!.Select(spt => new TrackMatch { Name = spt.Registrable!.DisplayName }))
                                                                 .ToArray()
                                                 })
                                  .ToListAsync(cancellationToken);

        return (unmatched, CalculateNode(unmatched));

    }

    private MenuNodeCalculation? CalculateNode(List<PotentialPartnerMatch> unmatched)
    {
        var node = new MenuNodeCalculation
                   {
                       Key = MenuNodeKey.AssignPartners
                   };

        if (unmatched.Count > 0)
        {
            node.Content = $"{unmatched.Count}";
            node.Style = MenuNodeStyle.ToDo;
        }

        return node;
    }
}