using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations
{
    public class SwapFirstLastNameCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrationId { get; set; }
    }

    public class SwapFirstLastNameCommandHandler : IRequestHandler<SwapFirstLastNameCommand>
    {
        private readonly EventContext _eventContext;
        private readonly IRepository<Registration> _registrations;

        public SwapFirstLastNameCommandHandler(IRepository<Registration> registrations,
                                               EventContext eventContext)
        {
            _registrations = registrations;
            _eventContext = eventContext;
        }

        public async Task<Unit> Handle(SwapFirstLastNameCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == _eventContext.EventId, cancellationToken);
            var firstName = registration.RespondentFirstName;
            registration.RespondentFirstName = registration.RespondentLastName;
            registration.RespondentLastName = firstName;

            return Unit.Value;
        }
    }
}