using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class LanguagesQuery : IRequest<IEnumerable<LanguageItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}