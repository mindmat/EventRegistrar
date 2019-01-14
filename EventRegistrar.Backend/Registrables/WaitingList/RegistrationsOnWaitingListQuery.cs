using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class RegistrationsOnWaitingListQuery : IRequest<IEnumerable<WaitingListSpot>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class RegistrationsOnWaitingListQueryHandler : IRequestHandler<RegistrationsOnWaitingListQuery, IEnumerable<WaitingListSpot>>
    {
        private readonly IQueryable<Seat> _seats;

        public RegistrationsOnWaitingListQueryHandler(IQueryable<Seat> seats)
        {
            _seats = seats;
        }

        public async Task<IEnumerable<WaitingListSpot>> Handle(RegistrationsOnWaitingListQuery query, CancellationToken cancellationToken)
        {
            return await _seats.Where(spt => spt.Registrable.EventId == query.EventId
                                          && spt.IsWaitingList
                                          && !spt.IsCancelled)
                               .Select(spt => new WaitingListSpot
                               {
                                   Leader = spt.RegistrationId == null ? null : new WaitingListRegistration
                                   {
                                       Id = spt.Registration.Id,
                                       State = spt.Registration.State,
                                       FirstName = spt.Registration.RespondentFirstName,
                                       LastName = spt.Registration.RespondentLastName,
                                       OptionsSent = spt.Registration.Mails.Any(map => map.Mail.Type == MailType.OptionsForRegistrationsOnWaitingList)
                                   },
                                   Follower = spt.RegistrationId_Follower == null ? null : new WaitingListRegistration
                                   {
                                       Id = spt.Registration_Follower.Id,
                                       State = spt.Registration_Follower.State,
                                       FirstName = spt.Registration_Follower.RespondentFirstName,
                                       LastName = spt.Registration_Follower.RespondentLastName,
                                       OptionsSent = spt.Registration_Follower.Mails.Any(map => map.Mail.Type == MailType.OptionsForRegistrationsOnWaitingList)
                                   },
                                   PlaceholderPartner = spt.IsPartnerSpot && (spt.RegistrationId == null || spt.RegistrationId_Follower == null)
                                                                                   ? spt.PartnerEmail
                                                                                   : null,
                                   IsOnWaitingList = spt.IsWaitingList || spt.Registration != null && spt.Registration.IsWaitingList == true,
                                   IsPartnerRegistration = spt.IsPartnerSpot || spt.PartnerEmail != null,
                                   RegistrableName = spt.Registrable.Name,
                                   Joined = spt.FirstPartnerJoined
                               })
                               .OrderBy(spt => spt.Joined)
                               .ToListAsync(cancellationToken);
        }
    }
}