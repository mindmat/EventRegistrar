using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionToRegistrablesQueryHandler : IRequestHandler<QuestionToRegistrablesQuery, IEnumerable<QuestionToRegistrablesDisplayItem>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<QuestionOptionToRegistrableMapping> _questionOptionsToRegistrables;

        public QuestionToRegistrablesQueryHandler(IQueryable<QuestionOptionToRegistrableMapping> questionOptionsToRegistrables,
                                                  IEventAcronymResolver acronymResolver)
        {
            _questionOptionsToRegistrables = questionOptionsToRegistrables;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<QuestionToRegistrablesDisplayItem>> Handle(QuestionToRegistrablesQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            return await _questionOptionsToRegistrables.Where(map => map.Registrable.EventId == eventId)
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