using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Search
{
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
            var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
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
                                                        StateText = reg.State.ToString(),
                                                        IsWaitingList = reg.IsWaitingList == true
                                                    })
                                                    .ToListAsync(cancellationToken);
            return registrations;
        }
    }
}