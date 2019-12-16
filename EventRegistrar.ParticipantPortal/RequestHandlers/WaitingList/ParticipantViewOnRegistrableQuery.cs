using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.ParticipantPortal.RequestHandlers.WaitingList
{
    public class ParticipantViewOnRegistrableQuery : IRequest<RegistrableView>
    {
        public Guid RegistrationId { get; set; }
    }

    public class RegistrableView
    {
        public string Name { get; set; }
    }

    public class ParticipantViewOnRegistrableQueryHandler : IRequestHandler<ParticipantViewOnRegistrableQuery, RegistrableView>
    {
        private readonly IQueryable<Registration> _registrations;

        public ParticipantViewOnRegistrableQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<RegistrableView> Handle(ParticipantViewOnRegistrableQuery query, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(rbl => rbl.Id == query.RegistrationId).FirstAsync();
            return new RegistrableView { Name = registration.RespondentLastName };
        }
    }
}
