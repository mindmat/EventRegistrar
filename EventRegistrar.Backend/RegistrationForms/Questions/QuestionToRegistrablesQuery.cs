using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionToRegistrablesQuery : IRequest<IEnumerable<QuestionToRegistrablesDisplayItem>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
    }
}