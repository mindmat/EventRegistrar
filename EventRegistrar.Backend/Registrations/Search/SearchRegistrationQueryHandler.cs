using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class SearchRegistrationQueryHandler : IRequestHandler<SearchRegistrationQuery, IEnumerable<RegistrationMatch>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registration> _registrations;
        private readonly IQueryable<Response> _responses;

        public SearchRegistrationQueryHandler(IQueryable<Response> responses,
                                              IQueryable<Registration> registrations,
                                              IEventAcronymResolver acronymResolver)
        {
            _responses = responses;
            _registrations = registrations;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<RegistrationMatch>> Handle(SearchRegistrationQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);

            var responseMatches = await _responses.Where(rsp => rsp.Registration.RegistrationForm.EventId == eventId
                                                             && rsp.ResponseString.Contains(query.SearchString))
                                                  .Select(rsp => new { rsp.RegistrationId, rsp.Question.Title, rsp.ResponseString })
                                                  .ToListAsync(cancellationToken);
            var registrationIds = responseMatches.Select(rsp => rsp.RegistrationId).Distinct().ToList();

            var results = await _registrations.Where(reg => reg.RegistrationForm.EventId == eventId)
                                              .Where(reg => reg.RespondentEmail.Contains(query.SearchString) ||
                                                            registrationIds.Contains(reg.Id))
                                              .Select(reg => new RegistrationMatch
                                              {
                                                  Id = reg.Id,
                                                  Email = reg.RespondentEmail,
                                                  FirstName = reg.RespondentFirstName,
                                                  LastName = reg.RespondentLastName,
                                                  Price = reg.Price ?? 0m
                                              })
                                              .ToListAsync(cancellationToken);

            foreach (var registrationMatch in results)
            {
                registrationMatch.Responses = responseMatches.Where(rsp => rsp.RegistrationId == registrationMatch.Id)
                                                             .Select(rsp => new ResponseMatch { Question = rsp.Title, Response = rsp.ResponseString })
                                                             .ToList();
            }

            return results;
        }
    }
}