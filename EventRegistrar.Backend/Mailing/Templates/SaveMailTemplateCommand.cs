using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class SaveMailTemplateCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public MailTemplateItem Template { get; set; }
    }
}