using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionToRegistrablesQueryHandler : IRequestHandler<QuestionToRegistrablesQuery, IEnumerable<QuestionToRegistrablesDisplayItem>>
    {
        private readonly IQueryable<QuestionOptionToRegistrableMapping> _questionOptionsToRegistrables;

        public QuestionToRegistrablesQueryHandler(IQueryable<QuestionOptionToRegistrableMapping> questionOptionsToRegistrables)
        {
            _questionOptionsToRegistrables = questionOptionsToRegistrables;
        }

        public async Task<IEnumerable<QuestionToRegistrablesDisplayItem>> Handle(QuestionToRegistrablesQuery query, CancellationToken cancellationToken)
        {
            return await _questionOptionsToRegistrables.Where(map => map.Registrable.EventId == query.EventId)
                                                       .OrderBy(map => map.QuestionOption.Question.Index)
                                                       .Select(map => new QuestionToRegistrablesDisplayItem
                                                       {
                                                           RegistrableId = map.RegistrableId,
                                                           RegistrableName = map.Registrable.Name,
                                                           QuestionOptionId = map.QuestionOptionId,
                                                           Question = map.QuestionOption.Question.Title,
                                                           Answer = map.QuestionOption.Answer
                                                       })
                                                       .ToListAsync(cancellationToken);
        }
    }
}