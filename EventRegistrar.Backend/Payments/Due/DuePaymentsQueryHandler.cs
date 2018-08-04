using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Sms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Due
{
    public class DuePaymentsQueryHandler : IRequestHandler<DuePaymentsQuery, IEnumerable<DuePaymentItem>>
    {
        public const int DefaultPaymentGracePeriod = 6;
        public static readonly HashSet<MailType?> MailTypes_Accepted = new HashSet<MailType?> { MailType.DoubleRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted };
        public static readonly HashSet<MailType?> MailTypes_Reminder1 = new HashSet<MailType?> { MailType.DoubleRegistrationFirstReminder, MailType.SingleRegistrationFirstReminder };
        public static readonly HashSet<MailType?> MailTypes_Reminder2 = new HashSet<MailType?> { MailType.DoubleRegistrationSecondReminder, MailType.SingleRegistrationSecondReminder };
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IQueryable<Registration> _registrations;

        public DuePaymentsQueryHandler(IQueryable<Registration> registrations,
                                       IEventAcronymResolver eventAcronymResolver)
        {
            _registrations = registrations;
            _eventAcronymResolver = eventAcronymResolver;
        }

        public async Task<IEnumerable<DuePaymentItem>> Handle(DuePaymentsQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _eventAcronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var reminderDueFrom = DateTime.UtcNow.AddDays(-DefaultPaymentGracePeriod);
            var dueRegistrations = await _registrations
                                         .Where(reg => reg.RegistrationForm.EventId == eventId &&
                                                       reg.State == RegistrationState.Received &&
                                                       reg.IsWaitingList != true)
                                         .Select(reg => new
                                         {
                                             reg.Id,
                                             FirstName = reg.RespondentFirstName,
                                             LastName = reg.RespondentLastName,
                                             Email = reg.RespondentEmail,
                                             reg.Price,
                                             reg.ReceivedAt,
                                             AcceptedMail = reg.Mails.Where(map => !map.Mail.Withhold &&
                                                                                   MailTypes_Accepted.Contains(map.Mail.Type))
                                                                     .Select(map => new SentMailDto
                                                                     {
                                                                         Id = map.MailId,
                                                                         Sent = map.Mail.Created
                                                                     })
                                                                     .OrderByDescending(mail => mail.Sent)
                                                                     .FirstOrDefault(),
                                             Reminder1Mail = reg.Mails.Where(map => !map.Mail.Withhold &&
                                                                                    MailTypes_Reminder1.Contains(map.Mail.Type))
                                                                      .Select(map => new SentMailDto
                                                                      {
                                                                          Id = map.MailId,
                                                                          Sent = map.Mail.Created
                                                                      })
                                                                      .OrderByDescending(mail => mail.Sent)
                                                                      .FirstOrDefault(),
                                             Reminder2Mail = reg.Mails.Where(map => !map.Mail.Withhold &&
                                                                                    MailTypes_Reminder2.Contains(map.Mail.Type))
                                                                      .Select(map => new SentMailDto
                                                                      {
                                                                          Id = map.MailId,
                                                                          Sent = map.Mail.Created
                                                                      })
                                                                      .OrderByDescending(mail => mail.Sent)
                                                                      .FirstOrDefault(),
                                             ReminderSmsSent = reg.Sms.Where(sms => sms.Type == SmsType.Reminder)
                                                                      .Select(sms => sms.Sent)
                                                                      .FirstOrDefault(),
                                             reg.PhoneNormalized,
                                             reg.ReminderLevel,
                                             Paid = (decimal?)reg.Payments.Sum(ass => ass.Amount)
                                         })
                                         .OrderBy(reg => reg.AcceptedMail == null ? DateTime.MaxValue : reg.AcceptedMail.Sent)
                                         .Select(reg => new DuePaymentItem
                                         {
                                             Id = reg.Id,
                                             FirstName = reg.FirstName,
                                             LastName = reg.LastName,
                                             Email = reg.Email,
                                             Price = reg.Price,
                                             Paid = reg.Paid,
                                             ReceivedAt = reg.ReceivedAt,
                                             AcceptedMail = reg.AcceptedMail,
                                             Reminder1Mail = reg.Reminder1Mail,
                                             Reminder2Mail = reg.Reminder2Mail,
                                             ReminderLevel = reg.ReminderLevel,
                                             Reminder1Due = reg.Reminder1Mail == null &&
                                                            reg.AcceptedMail != null &&
                                                            reg.AcceptedMail.Sent < reminderDueFrom,
                                             Reminder2Due = reg.Reminder2Mail == null &&
                                                            reg.Reminder1Mail != null &&
                                                            reg.Reminder1Mail.Sent < reminderDueFrom,
                                             ReminderSmsSent = reg.ReminderSmsSent,
                                             PhoneNormalized = reg.PhoneNormalized,
                                             ReminderSmsPossible = !reg.ReminderSmsSent.HasValue &&
                                                                   reg.AcceptedMail.Sent < reminderDueFrom &&
                                                                   reg.PhoneNormalized != null
                                         })
                                         .ToListAsync(cancellationToken);
            return dueRegistrations;
        }
    }

    public class SentMailDto
    {
        public Guid Id { get; set; }
        public DateTime Sent { get; set; }
    }
}