using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionToRegistrablesQuery : IRequest<IEnumerable<QuestionToRegistrablesDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}