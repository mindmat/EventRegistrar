using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class SetQuestionOptionToRegistrableMappingAttributesCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid QuestionOptionToRegistrableMappingId { get; set; }
        public QuestionMappingAttributes Attributes { get; set; }
    }

    public class SetQuestionOptionToRegistrableMappingAttributesCommandHandler : IRequestHandler<SetQuestionOptionToRegistrableMappingAttributesCommand>
    {
        private readonly IRepository<QuestionOptionMapping> _mappings;

        public SetQuestionOptionToRegistrableMappingAttributesCommandHandler(IRepository<QuestionOptionMapping> mappings)
        {
            _mappings = mappings;
        }

        public async Task<Unit> Handle(SetQuestionOptionToRegistrableMappingAttributesCommand command, CancellationToken cancellationToken)
        {
            var mapping = await _mappings.FirstAsync(map => map.Id == command.QuestionOptionToRegistrableMappingId
                                                         && map.QuestionOption.Question.RegistrationForm.EventId == command.EventId);
            if (command.Attributes.QuestionId_Partner != null)
            {
                mapping.QuestionId_Partner = command.Attributes.QuestionId_Partner;
            }
            if (command.Attributes.QuestionOptionId_Leader != null)
            {
                mapping.QuestionOptionId_Leader = command.Attributes.QuestionOptionId_Leader;
            }
            if (command.Attributes.QuestionOptionId_Follower != null)
            {
                mapping.QuestionOptionId_Follower = command.Attributes.QuestionOptionId_Follower;
            }
            return Unit.Value;
        }
    }
}
