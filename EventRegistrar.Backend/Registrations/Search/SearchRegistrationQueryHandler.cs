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

            var registrations = await _registrations.Where(reg => reg.EventId == eventId
                                                                  && (reg.RespondentFirstName.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.RespondentLastName.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.RespondentEmail.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase)
                                                                   || reg.PhoneNormalized.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
                                                                  && reg.State == RegistrationState.Received)
                                                    .Select(reg => new RegistrationMatch
                                                    {
                                                        RegistrationId = reg.Id,
                                                        FirstName = reg.RespondentFirstName,
                                                        LastName = reg.RespondentLastName,
                                                        Amount = reg.Price ?? 0m,
                                                        AmountPaid = reg.Payments.Sum(pmt => pmt.Amount),
                                                        State = reg.State
                                                    })
                                                    .ToListAsync(cancellationToken);
            return registrations;

            //var matches = await _responses.Where(rsp => rsp.Registration.EventId == eventId
            //                                         && rsp.ResponseString.Contains(query.SearchString))
            //                              .Select(rsp => new { rsp.RegistrationId, rsp.Question.Title, rsp.ResponseString })
            //                              .ToListAsync(cancellationToken);
            //var registrationMatches = await _registrations.Where(reg => reg.EventId == eventId
            //                                                        && (reg.RespondentLastName.Contains(query.SearchString)
            //                                                         || reg.RespondentFirstName.Contains(query.SearchString)
            //                                                         || reg.RespondentEmail.Contains(query.SearchString)
            //                                                         || reg.PhoneNormalized.Contains(query.SearchString)))
            //                                             .ToListAsync(cancellationToken);
            //matches.AddRange(registrationMatches
            //                 .Where(mat => mat.RespondentLastName.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
            //                 .Select(mat => new { RegistrationId = mat.Id, Title = Resources.LastName, ResponseString = mat.RespondentLastName }));
            //matches.AddRange(registrationMatches
            //                 .Where(mat => mat.RespondentFirstName.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
            //                 .Select(mat => new { RegistrationId = mat.Id, Title = Resources.FirstName, ResponseString = mat.RespondentFirstName }));
            //matches.AddRange(registrationMatches
            //                 .Where(mat => mat.RespondentEmail.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
            //                 .Select(mat => new { RegistrationId = mat.Id, Title = Resources.EMail, ResponseString = mat.RespondentEmail }));
            //matches.AddRange(registrationMatches
            //                 .Where(mat => mat.PhoneNormalized.Contains(query.SearchString, StringComparison.InvariantCultureIgnoreCase))
            //                 .Select(mat => new { RegistrationId = mat.Id, Title = Resources.Phone, ResponseString = mat.PhoneNormalized }));
            //var registrationIds = matches.Select(rsp => rsp.RegistrationId).Distinct().ToList();

            //var results = await _registrations.Where(reg => reg.RegistrationForm.EventId == eventId)
            //                                  .Where(reg => reg.RespondentEmail.Contains(query.SearchString) ||
            //                                                registrationIds.Contains(reg.Id))
            //                                  .Select(reg => new RegistrationMatch
            //                                  {
            //                                      Id = reg.Id,
            //                                      Email = reg.RespondentEmail,
            //                                      FirstName = reg.RespondentFirstName,
            //                                      LastName = reg.RespondentLastName,
            //                                      Price = reg.Price ?? 0m
            //                                  })
            //                                  .ToListAsync(cancellationToken);
        }
    }
}