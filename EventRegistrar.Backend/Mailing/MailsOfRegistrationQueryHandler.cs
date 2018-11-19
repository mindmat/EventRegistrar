using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsOfRegistrationQueryHandler : IRequestHandler<MailsOfRegistrationQuery, IEnumerable<Mail>>
    {
        private readonly IQueryable<MailToRegistration> _mails;

        public MailsOfRegistrationQueryHandler(IQueryable<MailToRegistration> mails)
        {
            _mails = mails;
        }

        public async Task<IEnumerable<Mail>> Handle(MailsOfRegistrationQuery query, CancellationToken cancellationToken)
        {
            var mails = await _mails
                              .Where(mail => mail.Registration.EventId == query.EventId
                                          && mail.RegistrationId == query.RegistrationId
                                          && !mail.Mail.Discarded)
                              .Select(mail => mail.Mail)
                              .OrderByDescending(mail => mail.Created)
                              .ToListAsync(cancellationToken);
            return mails;
        }
    }
}