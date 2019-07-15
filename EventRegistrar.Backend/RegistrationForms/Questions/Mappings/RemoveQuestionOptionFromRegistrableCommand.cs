using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class RemoveQuestionOptionFromRegistrableCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrableId { get; set; }
        public Guid QuestionOptionId { get; set; }
    }

    public class RemoveQuestionOptionFromRegistrableCommandHandler : IRequestHandler<RemoveQuestionOptionFromRegistrableCommand>
    {
        private readonly IRepository<QuestionOptionToRegistrableMapping> _mappings;

        public RemoveQuestionOptionFromRegistrableCommandHandler(IRepository<QuestionOptionToRegistrableMapping> mappings)
        {
            _mappings = mappings;
        }

        public async Task<Unit> Handle(RemoveQuestionOptionFromRegistrableCommand command, CancellationToken cancellationToken)
        {
            var mapping = await _mappings.FirstAsync(map => map.RegistrableId == command.RegistrableId
                                                         && map.QuestionOptionId == command.QuestionOptionId
                                                         && map.Registrable.EventId == command.EventId
                                                         && map.QuestionOption.Question.RegistrationForm.EventId == command.EventId);
            if (mapping != null)
            {
                _mappings.Remove(mapping);
            }

            return Unit.Value;
        }
    }
}
