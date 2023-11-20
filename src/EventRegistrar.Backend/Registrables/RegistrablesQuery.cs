namespace EventRegistrar.Backend.Registrables;

public class RegistrablesQuery : IRequest<IEnumerable<RegistrableDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrablesQueryHandler(IQueryable<Registrable> _registrables) : IRequestHandler<RegistrablesQuery, IEnumerable<RegistrableDisplayItem>>
{
    public async Task<IEnumerable<RegistrableDisplayItem>> Handle(RegistrablesQuery query,
                                                                  CancellationToken cancellationToken)
    {
        var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId)
                                              .Select(rbl => new RegistrableDisplayItem
                                                             {
                                                                 Id = rbl.Id,
                                                                 Name = rbl.Name,
                                                                 NameSecondary = rbl.NameSecondary,
                                                                 DisplayName = rbl.DisplayName,
                                                                 HasWaitingList = rbl.HasWaitingList,
                                                                 IsDoubleRegistrable = rbl.MaximumDoubleSeats.HasValue,
                                                                 ShowInMailListOrder = rbl.ShowInMailListOrder
                                                             })
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .ToListAsync(cancellationToken);

        return registrables;
    }
}