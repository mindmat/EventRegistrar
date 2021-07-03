using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Hosting
{
    public class HostingOffersQuery : IRequest<HostingOffers>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class HostingOffersQueryHandler : IRequestHandler<HostingOffersQuery, HostingOffers>
    {
        private readonly HostingConfiguration _configuration;
        private readonly IQueryable<Registration> _registrations;

        public HostingOffersQueryHandler(HostingConfiguration configuration,
                                         IQueryable<Registration> registrations)
        {
            _configuration = configuration;
            _registrations = registrations;
        }

        public async Task<HostingOffers> Handle(HostingOffersQuery query, CancellationToken cancellationToken)
        {
            var hostingOffers = await _registrations.Where(reg => reg.EventId == query.EventId
                                                               && reg.IsWaitingList == false
                                                               && (reg.State == RegistrationState.Received
                                                                || reg.State == RegistrationState.Paid)
                                                               && reg.Seats_AsLeader.Any(spt => !spt.IsCancelled
                                                                                             && !spt.IsWaitingList
                                                                                             && spt.RegistrableId == _configuration.RegistrableId_HostingOffer))
                                                    .Include(reg => reg.Responses)
                                                    .ToListAsync(cancellationToken);
            return new HostingOffers
            {
                DynamicColumns = _configuration.ColumnsOffers.Select(col => col.Key),
                Offers = hostingOffers
                         .Select(reg => new HostingOffer
                         {
                             RegistrationId = reg.Id,
                             FirstName = reg.RespondentFirstName,
                             LastName = reg.RespondentLastName,
                             Email = reg.RespondentEmail,
                             Phone = reg.Phone,
                             Language = reg.Language,
                             State = reg.State.ToString(),
                             AdmittedAt = reg.AdmittedAt,
                             Columns = _configuration
                                       .ColumnsOffers
                                       .ToDictionary(col => col.Key,
                                                     col => reg.Responses
                                                               .FirstOrDefault(rsp => rsp.QuestionId == col.Value)
                                                               ?.ResponseString)
                         })
                         .OrderBy(reg => reg.AdmittedAt ?? DateTime.MaxValue)
                         .ToList()
            };
        }
    }
}