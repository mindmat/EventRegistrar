using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Hosting
{
    public class HostingRequestsQueryHandler : IRequestHandler<HostingRequestsQuery, HostingRequests>
    {
        private readonly HostingConfiguration _configuration;
        private readonly IQueryable<Registration> _registrations;

        public HostingRequestsQueryHandler(HostingConfiguration configuration,
                                           IQueryable<Registration> registrations)
        {
            _configuration = configuration;
            _registrations = registrations;
        }

        public async Task<HostingRequests> Handle(HostingRequestsQuery query, CancellationToken cancellationToken)
        {
            var hostingRequests = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                 && reg.IsWaitingList == false
                                                                 && (reg.State == RegistrationState.Received
                                                                  || reg.State == RegistrationState.Paid)
                                                                 && reg.Seats_AsLeader.Any(spt => !spt.IsCancelled
                                                                                               && !spt.IsWaitingList
                                                                                               && spt.RegistrableId == _configuration.RegistrableId_HostingRequest))
                                                      .Include(reg => reg.Responses)
                                                      .ToListAsync(cancellationToken);
            return new HostingRequests
            {
                DynamicColumns = _configuration.ColumnsRequests.Select(col => col.Key),
                Requests = hostingRequests
                           .Select(reg => new HostingRequest
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
                                         .ColumnsRequests
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