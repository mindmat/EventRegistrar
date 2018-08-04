using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplatesQuery : IRequest<IEnumerable<MailTemplateItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}