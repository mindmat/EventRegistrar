using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Due
{
    public class DuePaymentsQuery : IRequest<IEnumerable<DuePaymentItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class DuePaymentsQueryHandler : IRequestHandler<DuePaymentsQuery, IEnumerable<DuePaymentItem>>
    {
        public const int DefaultPaymentGracePeriod = 6;
        public static readonly HashSet<MailType?> MailTypes_Accepted = new HashSet<MailType?> { MailType.PartnerRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted };
        public static readonly HashSet<MailType?> MailTypes_Reminder1 = new HashSet<MailType?> { MailType.PartnerRegistrationFirstReminder, MailType.SingleRegistrationFirstReminder };
        public static readonly HashSet<MailType?> MailTypes_Reminder2 = new HashSet<MailType?> { MailType.PartnerRegistrationSecondReminder, MailType.SingleRegistrationSecondReminder };
        private readonly IQueryable<Registration> _registrations;

        public DuePaymentsQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<DuePaymentItem>> Handle(DuePaymentsQuery query, CancellationToken cancellationToken)
        {
            var reminderDueFrom = DateTime.UtcNow.AddDays(-DefaultPaymentGracePeriod);
            var data = await _registrations.Where(reg => reg.RegistrationForm!.EventId == query.EventId
                                                      && reg.State == RegistrationState.Received
                                                      && reg.IsWaitingList != true
                                                      && reg.OriginalPrice > 0)
                                           .Select(reg => new
                                           {
                                               reg.Id,
                                               FirstName = reg.RespondentFirstName,
                                               LastName = reg.RespondentLastName,
                                               Email = reg.RespondentEmail,
                                               reg.Price,
                                               reg.ReceivedAt,
                                               reg.PhoneNormalized,
                                               reg.ReminderLevel,
                                               Paid = (decimal?)reg.Payments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                               Mails = reg.Mails.Select(rml => new { rml.Mail!.Id, Sent = rml.Mail.Created, rml.Mail.Type, rml.Mail.Withhold, rml.Mail.Discarded }),
                                               ReminderSms = reg.Sms.Where(sms => sms.Type == SmsType.Reminder)
                                           })
                                           .ToListAsync(cancellationToken);
            var dueRegistrations = data.Select(tmp => new
            {
                tmp.Id,
                tmp.FirstName,
                tmp.LastName,
                tmp.Email,
                tmp.Price,
                tmp.ReceivedAt,
                tmp.PhoneNormalized,
                tmp.ReminderLevel,
                tmp.Paid,
                AcceptedMail = tmp.Mails.Where(mail => !mail.Withhold
                                                    && !mail.Discarded
                                                    && MailTypes_Accepted.Contains(mail.Type))
                                        .Select(mail => new SentMailDto
                                        {
                                            Id = mail.Id,
                                            Sent = mail.Sent
                                        })
                                        .OrderByDescending(mail => mail.Sent)
                                        .FirstOrDefault(),
                Reminder1Mail = tmp.Mails.Where(mail => !mail.Withhold
                                                     && !mail.Discarded
                                                     && MailTypes_Reminder1.Contains(mail.Type))
                                         .Select(mail => new SentMailDto
                                         {
                                             Id = mail.Id,
                                             Sent = mail.Sent
                                         })
                                         .OrderByDescending(mail => mail.Sent)
                                         .FirstOrDefault(),
                Reminder2Mail = tmp.Mails.Where(mail => !mail.Withhold
                                                     && !mail.Discarded
                                                     && MailTypes_Reminder2.Contains(mail.Type))
                                         .Select(mail => new SentMailDto
                                         {
                                             Id = mail.Id,
                                             Sent = mail.Sent
                                         })
                                         .OrderByDescending(mail => mail.Sent)
                                         .FirstOrDefault(),

                ReminderSmsSent = tmp.ReminderSms.Select(sms => sms.Sent)
                                                 .FirstOrDefault()
            })
            .OrderBy(reg => reg.AcceptedMail?.Sent ?? DateTime.MaxValue)
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
                                      reg.AcceptedMail != null &&
                                      reg.AcceptedMail.Sent < reminderDueFrom &&
                                      reg.PhoneNormalized != null
            })
            .ToList();

            return dueRegistrations;
        }
    }

    public class SentMailDto
    {
        public Guid Id { get; set; }
        public DateTime Sent { get; set; }
    }
}