using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsOfRegistrationQueryHandler : IRequestHandler<MailsOfRegistrationQuery, IEnumerable<Mail>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<MailToRegistration> _mails;

        public MailsOfRegistrationQueryHandler(IQueryable<MailToRegistration> mails,
                                               IEventAcronymResolver acronymResolver)
        {
            _mails = mails;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<Mail>> Handle(MailsOfRegistrationQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);

            var mails = await _mails
                              .Where(mail => mail.Registration.EventId == eventId
                                          && mail.RegistrationId == query.RegistrationId)
                              .Select(mail => mail.Mail)
                              .OrderByDescending(mail => mail.Created)
                              .ToListAsync(cancellationToken);
            return mails;
        }
    }
}