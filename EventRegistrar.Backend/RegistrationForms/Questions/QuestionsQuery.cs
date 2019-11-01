using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class QuestionDisplayItem
    {
        public Guid Id { get; set; }
        public string Section { get; set; }
        public string Question { get; set; }
    }

    public class QuestionsQuery : IRequest<IEnumerable<QuestionDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid? FormId { get; set; }
    }

    public class QuestionsQueryHandler : IRequestHandler<QuestionsQuery, IEnumerable<QuestionDisplayItem>>
    {
        private readonly IQueryable<Question> _questions;

        public QuestionsQueryHandler(IQueryable<Question> questions)
        {
            _questions = questions;
        }

        public async Task<IEnumerable<QuestionDisplayItem>> Handle(QuestionsQuery query, CancellationToken cancellationToken)
        {
            return await _questions.Where(que => que.RegistrationForm.EventId == query.EventId
                                              && que.Type == QuestionType.Text)
                                   .WhereIf(query.FormId != null, que => que.RegistrationFormId == query.FormId)
                                   .Select(que => new QuestionDisplayItem
                                   {
                                       Id = que.Id,
                                       Section = que.Section,
                                       Question = que.Title
                                   })
                                   .ToListAsync();
        }
    }
}
