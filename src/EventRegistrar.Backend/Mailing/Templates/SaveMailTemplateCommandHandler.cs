using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates;

public class SaveMailTemplateCommandHandler : IRequestHandler<SaveMailTemplateCommand>
{
    private readonly IRepository<MailTemplate> _mailTemplates;

    public SaveMailTemplateCommandHandler(IRepository<MailTemplate> mailTemplates)
    {
        _mailTemplates = mailTemplates;
    }

    public async Task<Unit> Handle(SaveMailTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Template?.Template == null) throw new ArgumentException("no template provided");
        if (command.Template.Language == null) throw new ArgumentException("no language provided");
        if (command.Template.Subject == null) throw new ArgumentException("no subject provided");
        if (command.Template.SenderMail == null) throw new ArgumentException("no sender provided");

        command.Template.Language = command.Template.Language.ToLowerInvariant();
        MailTemplate template;
        if (command.Template.Type.HasValue && command.Template.Type != 0 &&
            !string.IsNullOrEmpty(command.Template.Language))
        {
            template = await _mailTemplates.FirstOrDefaultAsync(mtp => mtp.EventId == command.EventId
                                                                    && mtp.Type == command.Template.Type.Value
                                                                    && mtp.Language == command.Template.Language
                                                                    && mtp.BulkMailKey == null,
                cancellationToken);
        }
        else if (command.TemplateId.HasValue)
        {
            template = await _mailTemplates.FirstOrDefaultAsync(mtp => mtp.EventId == command.EventId
                                                                    && mtp.Type == 0
                                                                    && mtp.Id == command.TemplateId.Value,
                cancellationToken);
            if (template != null)
            {
                template.BulkMailKey = command.Template.Key;
                template.Language = command.Template.Language;
            }
        }
        else
        {
            throw new ArgumentException("Either id or type/language have to be provided");
        }

        if (template == null)
            template = new MailTemplate
                       {
                           Id = Guid.NewGuid(),
                           Language = command.Template.Language,
                           ContentType = MailContentType.Html,
                           EventId = command.EventId,
                           Type = command.Template.Type ?? 0,
                           BulkMailKey = command.Template.Key
                       };
        template.Template = command.Template.Template;
        template.SenderMail = command.Template.SenderMail;
        template.SenderName = command.Template.SenderName;
        template.Subject = command.Template.Subject;
        template.MailingAudience = command.Template.Audience;
        template.ReleaseImmediately = command.Template.ReleaseImmediately;
        await _mailTemplates.InsertOrUpdateEntity(template, cancellationToken);

        return Unit.Value;
    }
}