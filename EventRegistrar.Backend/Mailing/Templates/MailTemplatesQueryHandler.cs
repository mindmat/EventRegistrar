using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplatesQueryHandler : IRequestHandler<MailTemplatesQuery, IEnumerable<MailTemplateItem>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<MailTemplate> _mailTemplates;

        public MailTemplatesQueryHandler(IQueryable<MailTemplate> mailTemplates,
                                         IEventAcronymResolver acronymResolver)
        {
            _mailTemplates = mailTemplates;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<MailTemplateItem>> Handle(MailTemplatesQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var templates = await _mailTemplates
                .Where(mtp => mtp.Type == 0
                              && mtp.MailingKey != null
                              && mtp.EventId == eventId)
                .Select(mtp => new MailTemplateItem
                {
                    Key = mtp.MailingKey,
                    Language = mtp.Language,
                    Template = mtp.Template,
                    SenderMail = mtp.SenderMail,
                    SenderName = mtp.SenderName,
                    Subject = mtp.Subject
                })
                .ToListAsync(cancellationToken);

            return templates;
        }
    }
}