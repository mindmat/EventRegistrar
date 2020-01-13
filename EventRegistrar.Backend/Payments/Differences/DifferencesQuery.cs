using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Differences
{
    public class DifferencesQuery : IRequest<IEnumerable<DifferencesDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class DifferencesQueryHandler : IRequestHandler<DifferencesQuery, IEnumerable<DifferencesDisplayItem>>
    {
        private readonly IQueryable<Registration> _registrations;

        public DifferencesQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<DifferencesDisplayItem>> Handle(DifferencesQuery query, CancellationToken cancellationToken)
        {
            return await _registrations.Where(reg => reg.EventId == query.EventId
                                                  && (reg.State == RegistrationState.Received || reg.State == RegistrationState.Paid)
                                                  && reg.IsWaitingList == false
                                                  && reg.Price > 0m)
                                       .Select(reg => new
                                       {
                                           Registration = reg,
                                           PaymentsTotal = reg.Payments.Sum(pmt => pmt.Amount),
                                           Difference = (reg.Price ?? 0m) - reg.Payments.Sum(pmt => pmt.Amount)
                                       })
                                       .Where(reg => reg.Difference != 0m
                                                  && reg.PaymentsTotal > 0m)
                                       .OrderBy(reg => reg.Registration.AdmittedAt)
                                       .Select(reg => new DifferencesDisplayItem
                                       {
                                           RegistrationId = reg.Registration.Id,
                                           Price = reg.Registration.Price ?? 0m,
                                           AmountPaid = reg.PaymentsTotal,
                                           Difference = reg.Difference,
                                           FirstName = reg.Registration.RespondentFirstName,
                                           LastName = reg.Registration.RespondentLastName,
                                           State = reg.Registration.State
                                       })
                                       .ToListAsync(cancellationToken);
        }
    }

    public class DifferencesDisplayItem
    {
        public Guid RegistrationId { get; set; }
        public decimal Price { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Difference { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public RegistrationState State { get; set; }
    }
}