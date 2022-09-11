using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing.Templates;

public class UpdateAutoMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid TemplateId { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class UpdateAutoMailTemplateCommandHandler : IRequestHandler<UpdateAutoMailTemplateCommand>
{
    private readonly IRepository<AutoMailTemplate> _autoMailTemplates;

    public UpdateAutoMailTemplateCommandHandler(IRepository<AutoMailTemplate> autoMailTemplates)
    {
        _autoMailTemplates = autoMailTemplates;
    }

    public async Task<Unit> Handle(UpdateAutoMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _autoMailTemplates.AsTracking()
                                               .FirstAsync(mtp => mtp.EventId == command.EventId
                                                               && mtp.Id == command.TemplateId,
                                                           cancellationToken);
        template.Subject = command.Subject;
        if (!string.IsNullOrWhiteSpace(command.ContentHtml))
        {
            template.ContentHtml = command.ContentHtml;
        }

        return Unit.Value;
    }
}