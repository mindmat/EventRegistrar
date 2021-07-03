using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplatesQuery : IRequest<IEnumerable<MailTemplateItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public TemplateGroup TemplateGroup { get; set; }
    }
}