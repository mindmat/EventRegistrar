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
    public class QuestionToRegistrablesQuery : IRequest<IEnumerable<QuestionToRegistrablesDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

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
                                                           MappingId = map.Id,
                                                           RegistrableId = map.RegistrableId,
                                                           RegistrableName = map.Registrable.Name,
                                                           IsPartnerRegistrable = map.Registrable.MaximumDoubleSeats != null,
                                                           QuestionOptionId = map.QuestionOptionId,
                                                           Section = map.QuestionOption.Question.Section,
                                                           Question = map.QuestionOption.Question.Title,
                                                           Answer = map.QuestionOption.Answer,
                                                           QuestionId_Partner = map.QuestionId_Partner,
                                                           QuestionOptionId_Leader = map.QuestionOptionId_Leader,
                                                           QuestionOptionId_Follower = map.QuestionOptionId_Follower
                                                       })
                                                       .ToListAsync(cancellationToken);
        }
    }
}