using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Reductions
{
    public class SetReductionCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public bool IsReduced { get; set; }
        public Guid RegistrationId { get; set; }
    }

    public class SetReductionCommandHandler : IRequestHandler<SetReductionCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly IRepository<Registration> _registrations;

        public SetReductionCommandHandler(IRepository<Registration> registrations,
                                          IEventBus eventBus)
        {
            _registrations = registrations;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(SetReductionCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == command.EventId, cancellationToken);

            if (registration.IsReduced != command.IsReduced)
            {
                registration.IsReduced = command.IsReduced;
                _eventBus.Publish(new ReductionChanged
                {
                    EventId = registration.EventId,
                    RegistrationId = registration.Id,
                    IsReduced = registration.IsReduced
                });
            }
            // nothing to do
            return Unit.Value;
        }
    }
}