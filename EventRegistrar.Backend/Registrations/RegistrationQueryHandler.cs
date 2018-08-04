using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations
{
    public class RegistrationQueryHandler : IRequestHandler<RegistrationQuery, RegistrationDisplayItem>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registration> _registrations;

        public RegistrationQueryHandler(IQueryable<Registration> registrations,
                                        IEventAcronymResolver acronymResolver)
        {
            _registrations = registrations;
            _acronymResolver = acronymResolver;
        }

        public async Task<RegistrationDisplayItem> Handle(RegistrationQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);

            var registration = await _registrations.Where(reg => reg.EventId == eventId
                                                              && reg.Id == query.RegistrationId)
                                                   .Select(reg => new RegistrationDisplayItem
                                                   {
                                                       Id = reg.Id,
                                                       IsWaitingList = reg.IsWaitingList,
                                                       Price = reg.Price,
                                                       Status = reg.State,
                                                       StatusText = reg.State.ToString(),
                                                       Paid = (decimal?)reg.Payments.Sum(ass => ass.Amount) ?? 0m,
                                                       Language = reg.Language,
                                                       ReceivedAt = reg.ReceivedAt,
                                                       ReminderLevel = reg.ReminderLevel,
                                                       Remarks = reg.Remarks,
                                                       Email = reg.RespondentEmail,
                                                       FirstName = reg.RespondentFirstName,
                                                       LastName = reg.RespondentLastName,
                                                       SoldOutMessage = reg.SoldOutMessage,
                                                       FallbackToPartyPass = reg.FallbackToPartyPass,
                                                       SmsCount = reg.Sms.Count,
                                                       PhoneNormalized = reg.PhoneNormalized
                                                   })
                                                   .FirstOrDefaultAsync(cancellationToken);
            return registration;
        }
    }
}