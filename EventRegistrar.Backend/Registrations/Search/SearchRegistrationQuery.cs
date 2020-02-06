using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class SearchRegistrationQuery : IRequest<IEnumerable<RegistrationMatch>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public string SearchString { get; set; }
        public IEnumerable<RegistrationState> States { get; set; }
    }

    public class SearchRegistrationQueryHandler : IRequestHandler<SearchRegistrationQuery, IEnumerable<RegistrationMatch>>
    {
        private readonly IQueryable<Registration> _registrations;

        public SearchRegistrationQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<RegistrationMatch>> Handle(SearchRegistrationQuery query, CancellationToken cancellationToken)
        {
            var allowedStates = query.States.Any() ? query.States : new[] { RegistrationState.Received };
            var searchString = query.SearchString?.Trim();
            var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                               && (reg.RespondentFirstName.Contains(searchString)
                                                                || reg.RespondentLastName.Contains(searchString)
                                                                || reg.RespondentEmail.Contains(searchString)
                                                                || reg.PhoneNormalized != null && reg.PhoneNormalized.Contains(searchString))
                                                               && allowedStates.Contains(reg.State))
                                                    .Select(reg => new RegistrationMatch
                                                    {
                                                        RegistrationId = reg.Id,
                                                        FirstName = reg.RespondentFirstName,
                                                        LastName = reg.RespondentLastName,
                                                        Amount = reg.Price ?? 0m,
                                                        AmountPaid = reg.Payments.Select(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount).Sum(),
                                                        State = reg.State,
                                                        StateText = reg.State.ToString(),
                                                        IsWaitingList = reg.IsWaitingList == true,
                                                        //Registrables = reg.Seats_AsLeader.Select(spt => spt.Registrable.Name).Union(reg.Seats_AsFollower.Select(spt => spt.Registrable.Name))
                                                    })
                                                    .ToListAsync(cancellationToken);
            return registrations;
        }
    }
}