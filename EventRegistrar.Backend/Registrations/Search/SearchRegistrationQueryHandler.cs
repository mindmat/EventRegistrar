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
            var searchString = query.SearchString?.Trim();
            var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                  && (reg.RespondentFirstName.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.RespondentLastName.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.RespondentEmail.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.PhoneNormalized.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
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