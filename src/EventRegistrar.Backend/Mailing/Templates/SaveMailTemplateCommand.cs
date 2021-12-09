using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates;

public class SaveMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public MailTemplateItem Template { get; set; }
    public Guid? TemplateId { get; set; }
}