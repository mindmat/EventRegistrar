using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class PotentialPartnersQuery : IEventBoundRequest, IRequest<IEnumerable<PotentialPartnerMatch>>
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
        public string SearchString { get; set; }
    }

    public class PotentialPartnersQueryHandler : IRequestHandler<PotentialPartnersQuery, IEnumerable<PotentialPartnerMatch>>
    {
        private readonly IQueryable<Registration> _registrations;

        public PotentialPartnersQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<PotentialPartnerMatch>> Handle(PotentialPartnersQuery query, CancellationToken cancellationToken)
        {
            var ownRegistration = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                    && reg.Id == query.RegistrationId)
                                                      .Select(reg => new
                                                      {
                                                          reg.Id,
                                                          reg.PartnerNormalized,
                                                          PartnerSpotAsLeader = reg.Seats_AsLeader.FirstOrDefault(spt => spt.Registrable.MaximumDoubleSeats.HasValue),
                                                          PartnerSpotAsFollower = reg.Seats_AsFollower.FirstOrDefault(spt => spt.Registrable.MaximumDoubleSeats.HasValue)
                                                      })
                                                      .FirstAsync(cancellationToken);
            var searchParts = (query.SearchString ?? ownRegistration.PartnerNormalized)?.Split(" ");
            if (searchParts == null || searchParts.Length == 0)
            {
                throw new ArgumentException("No search string");
            }

            var partnerRegistrableId = ownRegistration.PartnerSpotAsLeader?.RegistrableId ?? ownRegistration.PartnerSpotAsFollower?.RegistrableId;
            if (!partnerRegistrableId.HasValue)
            {
                throw new ArgumentException("No partner spot found");
            }

            var otherRole = ownRegistration.PartnerSpotAsLeader != null ? Role.Follower : Role.Leader;
            return await _registrations.Where(reg => reg.EventId == query.EventId)
                                       .WhereIf(otherRole == Role.Leader, reg => reg.Seats_AsLeader.Any(spt => !spt.IsCancelled && spt.RegistrableId == partnerRegistrableId))
                                       .WhereIf(otherRole == Role.Follower, reg => reg.Seats_AsFollower.Any(spt => !spt.IsCancelled && spt.RegistrableId == partnerRegistrableId))
                                       .Select(reg => new
                                       {
                                           RegistrationId = reg.Id,
                                           Email = reg.RespondentEmail,
                                           FirstName = reg.RespondentFirstName,
                                           LastName = reg.RespondentLastName,
                                           State = reg.State.ToString(),
                                           Partner = reg.PartnerOriginal,
                                           EmailMatch = searchParts.Any(prt => prt == reg.RespondentEmail),
                                           FirstNameMatch = searchParts.Any(prt => prt == reg.RespondentFirstName),
                                           LastNameMatch = searchParts.Any(prt => prt == reg.RespondentLastName),
                                           reg.IsWaitingList,
                                           reg.RegistrationId_Partner,
                                           MatchedPartner = (reg.Registration_Partner.RespondentFirstName ?? string.Empty) + " " + (reg.Registration_Partner.RespondentLastName ?? string.Empty),
                                           Registrables = reg.Seats_AsLeader.Select(spt => spt.Registrable.Name).Union(reg.Seats_AsFollower.Select(spt => spt.Registrable.Name))
                                       })
                                       .Where(mat => mat.EmailMatch || mat.FirstNameMatch || mat.LastNameMatch)
                                       .OrderByDescending(mat => (mat.EmailMatch ? 5 : 0) +
                                                                 (mat.FirstNameMatch ? 1 : 0) +
                                                                 (mat.LastNameMatch ? 1 : 0))
                                       .Select(mat => new PotentialPartnerMatch
                                       {
                                           RegistrationId = mat.RegistrationId,
                                           Email = mat.Email,
                                           FirstName = mat.FirstName,
                                           LastName = mat.LastName,
                                           State = mat.State,
                                           Partner = mat.Partner,
                                           Registrables = mat.Registrables.ToArray(),
                                           IsWaitingList = mat.IsWaitingList == true,
                                           MatchedPartner = mat.MatchedPartner,
                                           RegistrationId_Partner = mat.RegistrationId_Partner
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }
}