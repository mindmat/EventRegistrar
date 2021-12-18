﻿using EventRegistrar.Backend.Authorization;

using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesQuery : IRequest<IEnumerable<RegistrableDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrablesQueryHandler : IRequestHandler<RegistrablesQuery, IEnumerable<RegistrableDisplayItem>>
{
    private readonly IQueryable<Registrable> _registrables;

    public RegistrablesQueryHandler(IQueryable<Registrable> registrables)
    {
        _registrables = registrables;
    }

    public async Task<IEnumerable<RegistrableDisplayItem>> Handle(RegistrablesQuery query,
                                                                  CancellationToken cancellationToken)
    {
        var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId)
                                              .Select(rbl => new RegistrableDisplayItem
                                                             {
                                                                 Id = rbl.Id,
                                                                 Name = rbl.Name,
                                                                 HasWaitingList = rbl.HasWaitingList,
                                                                 IsDoubleRegistrable = rbl.MaximumDoubleSeats.HasValue,
                                                                 ShowInMailListOrder = rbl.ShowInMailListOrder
                                                             })
                                              .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                              .ToListAsync(cancellationToken);

        return registrables;
    }
}