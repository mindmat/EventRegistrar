using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class MailsOfRegistrationQueryHandler : IRequestHandler<MailsOfRegistrationQuery, IEnumerable<MailDisplayItem>>
    {
        private readonly IQueryable<MailToRegistration> _mails;

        public MailsOfRegistrationQueryHandler(IQueryable<MailToRegistration> mails)
        {
            _mails = mails;
        }

        public async Task<IEnumerable<MailDisplayItem>> Handle(MailsOfRegistrationQuery query, CancellationToken cancellationToken)
        {
            var mails = await _mails
                              .Where(mtr => mtr.Registration.EventId == query.EventId
                                         && mtr.RegistrationId == query.RegistrationId
                                         && !mtr.Mail.Discarded)
                              .Select(mtr => new MailDisplayItem
                              {
                                  Id = mtr.MailId,
                                  Withhold = mtr.Mail.Withhold,
                                  SenderName = mtr.Mail.SenderName,
                                  SenderMail = mtr.Mail.SenderMail,
                                  Recipients = mtr.Mail.Recipients,
                                  Subject = mtr.Mail.Subject,
                                  Created = mtr.Mail.Created,
                                  ContentHtml = mtr.Mail.ContentHtml,
                                  State = mtr.Mail.State,
                                  Events = mtr.Mail.Events.OrderByDescending(mev => mev.Created).Select(mev => new MailEventDisplayItem
                                  {
                                      When = mev.Created,
                                      Email = mev.EMail,
                                      State = mev.State,
                                      StateText = mev.State.ToString()
                                  })
                              })
                              .OrderByDescending(mail => mail.Created)
                              .ToListAsync(cancellationToken);
            return mails;
        }
    }
}