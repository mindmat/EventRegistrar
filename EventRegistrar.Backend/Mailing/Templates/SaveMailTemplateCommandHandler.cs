using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class SaveMailTemplateCommandHandler : IRequestHandler<SaveMailTemplateCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IRepository<MailTemplate> _mailTemplates;

        public SaveMailTemplateCommandHandler(IRepository<MailTemplate> mailTemplates,
            IEventAcronymResolver acronymResolver)
        {
            _mailTemplates = mailTemplates;
            _acronymResolver = acronymResolver;
        }

        public async Task<Unit> Handle(SaveMailTemplateCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);
            var mailingKey = NormalizeKey(command.Template.Key);

            if (command.Template.Language == null)
            {
                throw new ArgumentException("no language provided");
            }
            if (command.Template.Template == null)
            {
                throw new ArgumentException("no template provided");
            }
            if (command.Template.Subject == null)
            {
                throw new ArgumentException("no subject provided");
            }
            if (command.Template.SenderMail == null)
            {
                throw new ArgumentException("no sender provided");
            }

            command.Template.Language = command.Template.Language.ToLowerInvariant();
            MailTemplate template;
            if (mailingKey != null)
            {
                template = await _mailTemplates.FirstOrDefaultAsync(mtp => mtp.EventId == eventId
                                                                        && mtp.Type == 0
                                                                        && mtp.Language == command.Template.Language
                                                                        && mtp.MailingKey == mailingKey
                                                                        && mtp.EventId == eventId,
                                                                    cancellationToken);
            }
            else if (command.Template.Type.HasValue)
            {
                template = await _mailTemplates.FirstOrDefaultAsync(mtp => mtp.EventId == eventId
                                                                        && mtp.Type == command.Template.Type.Value
                                                                        && mtp.Language == command.Template.Language
                                                                        && mtp.MailingKey == null
                                                                        && mtp.EventId == eventId,
                                                                    cancellationToken);
            }
            else
            {
                throw new ArgumentException("Either mailing key or type have to be provided");
            }

            if (template == null)
            {
                template = new MailTemplate
                {
                    Id = Guid.NewGuid(),
                    Language = command.Template.Language,
                    ContentType = MailContentType.Html,
                    EventId = eventId,
                    Type = command.Template.Type ?? 0,
                    MailingKey = mailingKey
                };
            }
            template.Template = command.Template.Template;
            template.SenderMail = command.Template.SenderMail;
            template.SenderName = command.Template.SenderName;
            template.Subject = command.Template.Subject;
            template.MailingAudience = command.Template.Audience;
            await _mailTemplates.InsertOrUpdateEntity(template, cancellationToken);

            return Unit.Value;
        }

        private static string NormalizeKey(string key)
        {
            return key?.ToLowerInvariant()?.Replace(" ", "");
        }
    }
}