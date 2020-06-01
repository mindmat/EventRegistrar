using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class UnassignedQuestionOptionsQuery : IRequest<IEnumerable<QuestionToRegistrablesDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class UnassignedQuestionOptionsQueryHandler : IRequestHandler<UnassignedQuestionOptionsQuery, IEnumerable<QuestionToRegistrablesDisplayItem>>
    {
        private readonly IQueryable<QuestionOption> _questionOptions;

        public UnassignedQuestionOptionsQueryHandler(IQueryable<QuestionOption> questionOptions)
        {
            _questionOptions = questionOptions;
        }

        public async Task<IEnumerable<QuestionToRegistrablesDisplayItem>> Handle(UnassignedQuestionOptionsQuery query, CancellationToken cancellationToken)
        {
            return await _questionOptions.Where(qop => qop.Question.RegistrationForm.EventId == query.EventId
                                                   && !qop.Registrables.Any(map => map.Registrable.RowVersion != null))
                                         .OrderBy(qop => qop.Question.Index)
                                         .Select(qop => new QuestionToRegistrablesDisplayItem
                                         {
                                             RegistrableId = null,
                                             RegistrableName = null,
                                             QuestionOptionId = qop.Id,
                                             Question = qop.Question.Title,
                                             Answer = qop.Answer,
                                             Section = qop.Question.Section
                                         })
                                         .ToListAsync(cancellationToken);
        }
    }
}