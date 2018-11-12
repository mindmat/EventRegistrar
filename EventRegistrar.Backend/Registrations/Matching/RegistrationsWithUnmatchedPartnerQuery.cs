using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class RegistrationsWithUnmatchedPartnerQuery : IEventBoundRequest, IRequest<IEnumerable<PotentialPartnerMatch>>
    {
        public Guid EventId { get; set; }
    }

    public class RegistrationsWithUnmatchedPartnerQueryHandler : IRequestHandler<RegistrationsWithUnmatchedPartnerQuery, IEnumerable<PotentialPartnerMatch>>
    {
        private readonly IQueryable<Registration> _registrations;

        public RegistrationsWithUnmatchedPartnerQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<PotentialPartnerMatch>> Handle(RegistrationsWithUnmatchedPartnerQuery query, CancellationToken cancellationToken)
        {
            return await _registrations.Where(reg => reg.EventId == query.EventId
                                                  && reg.State != RegistrationState.Cancelled
                                                  && reg.PartnerNormalized != null
                                                  && reg.RegistrationId_Partner == null)
                                       .OrderBy(reg => reg.ReceivedAt)
                                       .Select(reg => new PotentialPartnerMatch
                                       {
                                           RegistrationId = reg.Id,
                                           Email = reg.RespondentEmail,
                                           FirstName = reg.RespondentFirstName,
                                           LastName = reg.RespondentLastName,
                                           State = reg.State.ToString(),
                                           Partner = reg.PartnerOriginal,
                                           IsWaitingList = reg.IsWaitingList == true,
                                           Registrables = reg.Seats_AsLeader.Select(spt => spt.Registrable.Name).Union(reg.Seats_AsFollower.Select(spt => spt.Registrable.Name)).ToArray()
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }
}