using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates;

public class DeleteMailTemplateCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public Guid MailTemplateId { get; set; }
}

public class DeleteMailTemplateCommandHandler : IRequestHandler<DeleteMailTemplateCommand>
{
    private readonly IRepository<MailTemplate> _mailTemplates;

    public DeleteMailTemplateCommandHandler(IRepository<MailTemplate> mailTemplates)
    {
        _mailTemplates = mailTemplates;
    }

    public async Task<Unit> Handle(DeleteMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _mailTemplates.Where(mtp => mtp.Id == command.MailTemplateId
                                                      && mtp.EventId == command.EventId)
                                           .Include(mtp => mtp.Event)
                                           .FirstAsync();
        if (template.Event.State != RegistrationForms.State.Setup)
            throw new Exception(
                $"To delete a template, event must be in state Setup, but it is in state {template.Event.State}");
        template.IsDeleted = true;
        await _mailTemplates.InsertOrUpdateEntity(template);

        return Unit.Value;
    }
}