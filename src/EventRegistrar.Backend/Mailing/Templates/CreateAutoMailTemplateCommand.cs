using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Templates;

public class CreateAutoMailTemplateCommand : IRequest<Guid>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public MailType Type { get; set; }
    public string Language { get; set; } = null!;
}

public class CreateAutoMailTemplateCommandHandler(IRepository<AutoMailTemplate> templates,
                                                  IEventBus eventBus)
    : IRequestHandler<CreateAutoMailTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateAutoMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var templatesOfType = await templates.Where(mtp => mtp.EventId == command.EventId
                                                        && mtp.Type == command.Type)
                                             .ToListAsync(cancellationToken);
        var template = templatesOfType.FirstOrDefault(mtp => mtp.Language == command.Language);
        if (template != null)
        {
            return template.Id;
        }

        template = templates.InsertObjectTree(new AutoMailTemplate
                                              {
                                                  Id = Guid.NewGuid(),
                                                  EventId = command.EventId,
                                                  Type = command.Type,
                                                  Language = command.Language,
                                                  ReleaseImmediately = templatesOfType.FirstOrDefault()?.ReleaseImmediately ?? false // copy from other language
                                              });

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(AutoMailTemplatesQuery)
                         });

        return template.Id;
    }
}