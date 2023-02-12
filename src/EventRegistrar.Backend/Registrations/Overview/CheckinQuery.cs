using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables;

namespace EventRegistrar.Backend.Registrations.Overview;

public class CheckInQuery : IRequest<CheckInView>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class CheckInQueryHandler : IRequestHandler<CheckInQuery, CheckInView>
{
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<Registration> _registrations;

    public CheckInQueryHandler(IQueryable<Registration> registrations,
                               IQueryable<Registrable> registrables)
    {
        _registrations = registrations;
        _registrables = registrables;
    }

    public async Task<CheckInView> Handle(CheckInQuery query, CancellationToken cancellationToken)
    {
        var columns = (await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                     && rbl.CheckinListColumn != null)
                                          .Select(rbl => new { rbl.Id, rbl.DisplayName, rbl.CheckinListColumn })
                                          .ToListAsync(cancellationToken)
                      )
                      .GroupBy(rbl => rbl.CheckinListColumn)
                      .ToDictionary(grp => grp.Key, grp => grp.Select(rbl => new { rbl.Id, rbl.DisplayName }));

        var registrations = await _registrations.Where(reg => reg.RegistrationForm!.EventId == query.EventId
                                                           && reg.IsOnWaitingList == false
                                                           && reg.State != RegistrationState.Cancelled)
                                                .Select(reg => new
                                                               {
                                                                   RegistrationId = reg.Id,
                                                                   Email = reg.RespondentEmail,
                                                                   FirstName = reg.RespondentFirstName,
                                                                   LastName = reg.RespondentLastName,
                                                                   reg.State,
                                                                   reg.AdmittedAt,
                                                                   Price = reg.Price_AdmittedAndReduced,
                                                                   Payments = reg.PaymentAssignments!.Select(asn =>
                                                                                                                 asn.PayoutRequestId == null
                                                                                                                     ? asn.Amount
                                                                                                                     : -asn.Amount)
                                                                                 .ToList(),
                                                                   SeatsAsLeader = reg.Seats_AsLeader!.Where(seat => !seat.IsCancelled)
                                                                                      .ToList(),
                                                                   SeatsAsFollower = reg.Seats_AsFollower!
                                                                                        .Where(seat => !seat.IsCancelled)
                                                                                        .ToList()
                                                               })
                                                .ToListAsync(cancellationToken);

        var items = registrations.Select(reg => new CheckInViewItem
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
                                                                                .Select(rbl => rbl.DisplayName)
                                                                                .StringJoin())
                                                })
                                 .OrderBy(reg => reg.LastName)
                                 .ThenBy(reg => reg.FirstName)
                                 .ToList();

        return new CheckInView
               {
                   DynamicHeaders = columns.Keys,
                   Items = items
               };
    }
}

public class CheckInView
{
    public IEnumerable<string> DynamicHeaders { get; set; }
    public IEnumerable<CheckInViewItem> Items { get; set; }
}

public class CheckInViewItem
{
    public DateTimeOffset? AdmittedAt { get; set; }
    public IDictionary<string, string> Columns { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid RegistrationId { get; set; }
    public string Status { get; set; }
    public decimal UnsettledAmount { get; set; }
}