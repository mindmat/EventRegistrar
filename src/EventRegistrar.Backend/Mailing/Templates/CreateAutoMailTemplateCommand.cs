using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Templates;

public class CreateAutoMailTemplateCommand : IRequest<Guid>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public MailType Type { get; set; }
    public string Language { get; set; } = null!;
}

public class CreateAutoMailTemplateCommandHandler : IRequestHandler<CreateAutoMailTemplateCommand, Guid>
{
    private readonly IRepository<AutoMailTemplate> _templates;
    private readonly IEventBus _eventBus;

    public CreateAutoMailTemplateCommandHandler(IRepository<AutoMailTemplate> templates,
                                                IEventBus eventBus)
    {
        _templates = templates;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(CreateAutoMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _templates.Where(mtp => mtp.EventId == command.EventId
                                                  && mtp.Type == command.Type
                                                  && mtp.Language == command.Language)
                                       .FirstOrDefaultAsync(cancellationToken);
        if (template != null)
        {
            return template.Id;
        }

        template = _templates.InsertObjectTree(new AutoMailTemplate
                                               {
                                                   Id = Guid.NewGuid(),
                                                   EventId = command.EventId,
                                                   Type = command.Type,
                                                   Language = command.Language
                                               });

        _eventBus.Publish(new ReadModelUpdated
                          {
                              EventId = command.EventId,
                              QueryName = nameof(AutoMailTemplatesQuery)
                          });

        return template.Id;
    }
}