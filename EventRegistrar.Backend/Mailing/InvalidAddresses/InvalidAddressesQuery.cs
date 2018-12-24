using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses
{
    public class InvalidAddress
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public DateTime? LastMailSent { get; set; }
        public string LastMailState { get; set; }
        public string LastName { get; set; }
        public string Proposals { get; set; }
        public Guid RegistrationId { get; set; }
        public string RegistrationState { get; set; }
    }

    public class InvalidAddressesQuery : IRequest<IEnumerable<InvalidAddress>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class InvalidAddressesQueryHandler : IRequestHandler<InvalidAddressesQuery, IEnumerable<InvalidAddress>>
    {
        private static readonly MailState[] DeliveredMailStates = { MailState.Delivered, MailState.Open, MailState.Click };
        private readonly IQueryable<Registration> _registrations;

        public InvalidAddressesQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<InvalidAddress>> Handle(InvalidAddressesQuery query, CancellationToken cancellationToken)
        {
            var invalidMails = await _registrations.Where(reg => reg.EventId == query.EventId
                                                       && reg.State != RegistrationState.Cancelled
                                                       && reg.Mails.Any(mail => mail.State != null)
                                                       && !reg.Mails.Any(mail => mail.State != null && DeliveredMailStates.Contains(mail.State.Value)))
                                            .Select(reg => new InvalidAddress
                                            {
                                                RegistrationId = reg.Id,
                                                RegistrationState = reg.State.ToString(),
                                                FirstName = reg.RespondentFirstName,
                                                LastName = reg.RespondentLastName,
                                                Email = reg.RespondentEmail,
                                                LastMailSent = reg.Mails.Where(mail => mail.State != null)
                                                                        .OrderByDescending(mail => mail.Mail.Sent)
                                                                        .Select(mail => mail.Mail.Sent)
                                                                        .First(),
                                                LastMailState = reg.Mails.Where(mail => mail.State != null)
                                                                         .OrderByDescending(mail => mail.Mail.Sent)
                                                                         .Select(mail => mail.State)
                                                                         .First()
                                                                         .ToString(),
                                            })
                                            .OrderByDescending(mail => mail.LastMailSent)
                                            .ToListAsync(cancellationToken);
            foreach (var invalidMail in invalidMails)
            {
                var otherRegistrations = await _registrations.Where(reg => reg.Id != invalidMail.RegistrationId
                                                                        && reg.RespondentFirstName == invalidMail.FirstName
                                                                        && reg.RespondentLastName == invalidMail.LastName
                                                                        && reg.Mails.Any(mail => mail.State == null || DeliveredMailStates.Contains(mail.State.Value)))
                                                             .Select(reg => reg.RespondentEmail)
                                                             .ToListAsync(cancellationToken);
                invalidMail.Proposals = otherRegistrations.Distinct().StringJoin(";");
            }

            return invalidMails;
        }
    }
}