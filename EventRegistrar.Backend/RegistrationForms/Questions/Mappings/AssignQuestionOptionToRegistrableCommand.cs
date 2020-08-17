using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class AssignQuestionOptionToRegistrableCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrableId { get; set; }
        public Guid QuestionOptionId { get; set; }
    }

    public class AssignQuestionOptionToRegistrableCommandHandler : IRequestHandler<AssignQuestionOptionToRegistrableCommand>
    {
        private readonly IRepository<QuestionOptionMapping> _mappings;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IQueryable<QuestionOption> _questionOptions;

        public AssignQuestionOptionToRegistrableCommandHandler(IRepository<QuestionOptionMapping> mappings,
                                                               IQueryable<Registrable> registrables,
                                                               IQueryable<QuestionOption> questionOptions)
        {
            _mappings = mappings;
            _registrables = registrables;
            _questionOptions = questionOptions;
        }

        public async Task<Unit> Handle(AssignQuestionOptionToRegistrableCommand command, CancellationToken cancellationToken)
        {
            var registrable = _registrables.FirstOrDefault(rbl => rbl.Id == command.RegistrableId
                                                               && rbl.EventId == command.EventId);
            var questionOption = _questionOptions.FirstOrDefault(qop => qop.Id == command.QuestionOptionId
                                                                     && qop.Question.RegistrationForm.EventId == command.EventId);
            if (registrable == null
             || questionOption == null
             || await _mappings.AnyAsync(map => map.RegistrableId == command.RegistrableId
                                             && map.QuestionOptionId == command.QuestionOptionId, cancellationToken))
            {
                return Unit.Value;
            }

            await _mappings.InsertOrUpdateEntity(new QuestionOptionMapping
            {
                Id = Guid.NewGuid(),
                RegistrableId = command.RegistrableId,
                QuestionOptionId = command.QuestionOptionId
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
