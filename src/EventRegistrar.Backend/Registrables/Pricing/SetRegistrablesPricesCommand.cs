using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.Pricing
{
    public class SetRegistrablesPricesCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrableId { get; set; }
        public decimal? Price { get; set; }
        public decimal? ReducedPrice { get; set; }
    }

    public class SetRegistrablesPricesCommandHandler : IRequestHandler<SetRegistrablesPricesCommand>
    {
        private readonly IQueryable<Registrable> _registrables;

        public SetRegistrablesPricesCommandHandler(IQueryable<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<Unit> Handle(SetRegistrablesPricesCommand command, CancellationToken cancellationToken)
        {
            var registrable = await _registrables.FirstAsync(rbl => rbl.EventId == command.EventId
                                                                 && rbl.Id == command.RegistrableId);
            registrable.Price = command.Price;
            registrable.ReducedPrice = command.ReducedPrice;

            return Unit.Value;
        }
    }
}
