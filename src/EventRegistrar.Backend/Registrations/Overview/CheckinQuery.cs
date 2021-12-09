using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Overview;

public class CheckinQuery : IRequest<CheckinView>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class CheckinQueryHandler : IRequestHandler<CheckinQuery, CheckinView>
{
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<Registration> _registrations;

    public CheckinQueryHandler(IQueryable<Registration> registrations,
                               IQueryable<Registrable> registrables)
    {
        _registrations = registrations;
        _registrables = registrables;
    }

    public async Task<CheckinView> Handle(CheckinQuery query, CancellationToken cancellationToken)
    {
        var columns = (await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                     && rbl.CheckinListColumn != null)
                                          .Select(rbl => new { rbl.Id, rbl.Name, rbl.CheckinListColumn })
                                          .ToListAsync(cancellationToken)
                      )
                      .GroupBy(rbl => rbl.CheckinListColumn)
                      .ToDictionary(grp => grp.Key, grp => grp.Select(rbl => new { rbl.Id, rbl.Name }));
        var registrations = await _registrations
                                  .Where(reg => reg.RegistrationForm.EventId == query.EventId
                                             && reg.IsWaitingList == false
                                             && reg.State != RegistrationState.Cancelled)
                                  .Select(reg => new
                                                 {
                                                     RegistrationId = reg.Id,
                                                     Email = reg.RespondentEmail,
                                                     FirstName = reg.RespondentFirstName,
                                                     LastName = reg.RespondentLastName,
                                                     reg.State,
                                                     reg.AdmittedAt,
                                                     Price = reg.Price ?? 0m,
                                                     Payments = reg.Payments.Select(asn =>
                                                                       asn.PayoutRequestId == null
                                                                           ? asn.Amount
                                                                           : -asn.Amount)
                                                                   .ToList(),
                                                     SeatsAsLeader = reg.Seats_AsLeader.Where(seat => !seat.IsCancelled)
                                                                        .ToList(),
                                                     SeatsAsFollower = reg.Seats_AsFollower
                                                                          .Where(seat => !seat.IsCancelled)
                                                                          .ToList()
                                                 })
                                  .ToListAsync(cancellationToken);
        var items = registrations
                    .Select(reg => new CheckinViewItem
                                   {
                                       RegistrationId = reg.RegistrationId,
                                       Email = reg.Email,
                                       FirstName = reg.FirstName,
                                       LastName = reg.LastName,
                                       Status = reg.State.ToString(),
                                       AdmittedAt = reg.AdmittedAt,
                                       UnsettledAmount = reg.Price - reg.Payments.Sum(),
                                       Columns = columns
                                           .ToDictionary(col => col.Key,
                                               col => col.Value.Where(rbl =>
                                                             reg.SeatsAsLeader.Any(seat => seat.RegistrableId == rbl.Id)
                                                          || reg.SeatsAsFollower.Any(seat =>
                                                                 seat.RegistrableId == rbl.Id))
                                                         .Select(rbl => rbl.Name)
                                                         .StringJoin())
                                   })
                    .OrderBy(reg => reg.LastName)
                    .ThenBy(reg => reg.FirstName)
                    .ToList();

        return new CheckinView
               {
                   DynamicHeaders = columns.Keys,
                   Items = items
               };
    }
}