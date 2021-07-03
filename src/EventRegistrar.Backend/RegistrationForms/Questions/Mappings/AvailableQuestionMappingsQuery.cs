using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class AvailableQuestionMappingsQuery : IRequest<IEnumerable<AvailableQuestionMapping>>
    {
        public Guid EventId { get; set; }
    }

    public class AvailableQuestionMappingsQueryHandler : IRequestHandler<AvailableQuestionMappingsQuery, IEnumerable<AvailableQuestionMapping>>
    {
        public Task<IEnumerable<AvailableQuestionMapping>> Handle(AvailableQuestionMappingsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new[]
            {
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.FirstName,
                    Text = Properties.Resources.FirstName
                },
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.LastName,
                    Text = Properties.Resources.LastName
                },
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.EMail,
                    Text = Properties.Resources.EMail
                },
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.Phone,
                    Text = Properties.Resources.Phone
                },
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.Town,
                    Text = Properties.Resources.Town
                },
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.Partner,
                    Text = Properties.Resources.Partner
                },
                new AvailableQuestionMapping
                {
                    Type = QuestionMappingType.Remarks,
                    Text = Properties.Resources.Remarks
                },
            } as IEnumerable<AvailableQuestionMapping>);
        }
    }

    public class AvailableQuestionMapping
    {
        public QuestionMappingType Type { get; set; }
        public string Text { get; set; } = null!;
    }
}