using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionToRegistrablesDisplayItem
    {
        public string Answer { get; set; }
        public Guid QuestionOptionId { get; set; }
        public Guid RegistrableId { get; set; }
        public string RegistrableName { get; set; }
    }

    public class QuestionToRegistrablesQuery : IRequest<IEnumerable<QuestionToRegistrablesDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}