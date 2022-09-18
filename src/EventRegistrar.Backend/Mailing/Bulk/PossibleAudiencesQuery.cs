using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class PossibleAudience
{
    public MailingAudience Audience { get; set; }
    public string Name { get; set; }
}

public class PossibleAudiencesQuery : IRequest<IEnumerable<PossibleAudience>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PossibleAudiencesQueryHandler : IRequestHandler<PossibleAudiencesQuery, IEnumerable<PossibleAudience>>
{
    private readonly IQueryable<Event> _events;
    private readonly IQueryable<Registration> _registrations;

    public PossibleAudiencesQueryHandler(IQueryable<Event> events,
                                         IQueryable<Registration> registrations)
    {
        _events = events;
        _registrations = registrations;
    }

    public async Task<IEnumerable<PossibleAudience>> Handle(PossibleAudiencesQuery query,
                                                            CancellationToken cancellationToken)
    {
        var registrations = await _registrations.Where(reg => reg.EventId == query.EventId)
                                                .Select(reg => new
                                                               {
                                                                   reg.State,
                                                                   reg.IsWaitingList
                                                               })
                                                .ToListAsync(cancellationToken);
        var result = new List<PossibleAudience>
                     {
                         new()
                         {
                             Audience = MailingAudience.Paid,
                             Name = Properties.Resources.MailingAudience_Paid + $" ({registrations.Count(reg => reg.State == RegistrationState.Paid)})"
                         },
                         new()
                         {
                             Audience = MailingAudience.Unpaid,
                             Name = Properties.Resources.MailingAudience_Unpaid + $" ({registrations.Count(reg => reg.State == RegistrationState.Received && reg.IsWaitingList != true)})"
                         },
                         new()
                         {
                             Audience = MailingAudience.WaitingList,
                             Name = Properties.Resources.MailingAudience_WaitingList + $" ({registrations.Count(reg => reg.State == RegistrationState.Received && reg.IsWaitingList == true)})"
                         }
                     };
        var predecessorEvent = await _events.Where(evt => evt.Id == query.EventId)
                                            .Select(evt => new
                                                           {
                                                               evt.PredecessorEvent,
                                                               PredecessorEventRegistrationCount =
                                                                   evt.PredecessorEvent.Registrations.Count(),
                                                               PrePredecessorEvent =
                                                                   evt.PredecessorEvent.PredecessorEvent,
                                                               PrePredecessorEventRegistrationCount =
                                                                   evt.PredecessorEvent.PredecessorEvent.Registrations
                                                                      .Count()
                                                           })
                                            .FirstAsync();
        if (predecessorEvent?.PredecessorEvent != null)
        {
            result.Add(new PossibleAudience
                       {
                           Audience = MailingAudience.PredecessorEvent,
                           Name = predecessorEvent.PredecessorEvent.Name + $" ({predecessorEvent.PredecessorEventRegistrationCount})"
                       });
            if (predecessorEvent?.PrePredecessorEvent != null)
            {
                result.Add(new PossibleAudience
                           {
                               Audience = MailingAudience.PrePredecessorEvent,
                               Name = predecessorEvent.PrePredecessorEvent.Name + $" ({predecessorEvent.PrePredecessorEventRegistrationCount})"
                           });
            }
        }

        return result;
    }
}