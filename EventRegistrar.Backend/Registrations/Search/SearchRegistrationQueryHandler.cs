using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class SearchRegistrationQueryHandler : IRequestHandler<SearchRegistrationQuery, IEnumerable<RegistrationMatch>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registration> _registrations;

        public SearchRegistrationQueryHandler(IQueryable<Registration> registrations,
                                              IEventAcronymResolver acronymResolver)
        {
            _registrations = registrations;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<RegistrationMatch>> Handle(SearchRegistrationQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var allowedStates = query.States.Any() ? query.States : new[] { RegistrationState.Received };
            var registrations = await _registrations.Where(reg => reg.EventId == eventId
                                                                  && (reg.RespondentFirstName.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.RespondentLastName.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.RespondentEmail.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.PhoneNormalized.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
                                                                  && allowedStates.Contains(reg.State))
                                                    .Select(reg => new RegistrationMatch
                                                    {
                                                        RegistrationId = reg.Id,
                                                        FirstName = reg.RespondentFirstName,
                                                        LastName = reg.RespondentLastName,
                                                        Amount = reg.Price ?? 0m,
                                                        AmountPaid = reg.Payments.Sum(pmt => pmt.Amount),
                                                        State = reg.State,
                                                        StateText = reg.State.ToString()
                                                    })
                                                    .ToListAsync(cancellationToken);
            return registrations;
        }
    }
}