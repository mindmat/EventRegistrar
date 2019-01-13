using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class RegistrationsOnWaitingListQuery : IRequest<IEnumerable<PlaceDisplayInfo>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class RegistrationsOnWaitingListQueryHandler : IRequestHandler<RegistrationsOnWaitingListQuery, IEnumerable<PlaceDisplayInfo>>
    {
        private readonly IQueryable<Seat> _seats;

        public RegistrationsOnWaitingListQueryHandler(IQueryable<Seat> seats)
        {
            _seats = seats;
        }

        public async Task<IEnumerable<PlaceDisplayInfo>> Handle(RegistrationsOnWaitingListQuery query, CancellationToken cancellationToken)
        {
            return await _seats.Where(spt => spt.Registrable.EventId == query.EventId
                                          && spt.IsWaitingList)
                               .Select(spt => new PlaceDisplayInfo
                               {
                                   Leader = spt.RegistrationId == null ? null : new RegistrationDisplayInfo
                                   {
                                       Id = spt.Registration.Id,
                                       Email = spt.Registration.RespondentEmail,
                                       State = spt.Registration.State,
                                       FirstName = spt.Registration.RespondentFirstName,
                                       LastName = spt.Registration.RespondentLastName
                                   },
                                   Follower = spt.RegistrationId_Follower == null ? null : new RegistrationDisplayInfo
                                   {
                                       Id = spt.Registration_Follower.Id,
                                       Email = spt.Registration_Follower.RespondentEmail,
                                       State = spt.Registration_Follower.State,
                                       FirstName = spt.Registration_Follower.RespondentFirstName,
                                       LastName = spt.Registration_Follower.RespondentLastName
                                   },
                                   PlaceholderPartner = spt.IsPartnerSpot && (spt.RegistrationId == null || spt.RegistrationId_Follower == null)
                                                                                   ? spt.PartnerEmail
                                                                                   : null,
                                   IsOnWaitingList = spt.IsWaitingList || spt.Registration != null && spt.Registration.IsWaitingList == true,
                                   IsPartnerRegistration = spt.IsPartnerSpot || spt.PartnerEmail != null,
                                   Registrable = spt.Registrable.Name,
                                   Joined = spt.FirstPartnerJoined
                               })
                               .ToListAsync(cancellationToken);
        }
    }
}