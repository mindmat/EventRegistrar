using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class AvailableQuestionOptionMappingsQuery : IRequest<IEnumerable<QuestionOptionMapping>>
    {
        public Guid EventId { get; set; }
    }

    public class AvailableQuestionOptionMappingsQueryHandler : IRequestHandler<AvailableQuestionOptionMappingsQuery, IEnumerable<QuestionOptionMapping>>
    {
        //public Guid ReductionId = Guid.Parse("13B798C3-CA92-4533-B12F-B7A0E83DD021");

        private readonly IQueryable<Registrable> _registrables;

        public AvailableQuestionOptionMappingsQueryHandler(IQueryable<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<IEnumerable<QuestionOptionMapping>> Handle(AvailableQuestionOptionMappingsQuery request, CancellationToken cancellationToken)
        {
            var result = new List<QuestionOptionMapping>();
            var doubleRegistrables = await _registrables.Where(rbl => rbl.EventId == request.EventId
                                                                   && rbl.MaximumDoubleSeats != null)
                                                        .Select(rbl => new
                                                        {
                                                            rbl.Id,
                                                            rbl.Name,
                                                            SortKey = rbl.ShowInMailListOrder ?? int.MaxValue
                                                        })
                                                        .ToListAsync(cancellationToken);

            result.AddRange(doubleRegistrables.OrderBy(rbl => rbl.SortKey)
                                              .SelectMany(rbl =>
                                              {
                                                  return new[]
                                                  {
                                                     new QuestionOptionMapping
                                                     {
                                                         Id = rbl.Id,
                                                         Type = MappingType.DoubleRegistrableLeader,
                                                         Name = $"{rbl.Name} (Leader)"
                                                     },
                                                     new QuestionOptionMapping
                                                     {
                                                         Id = rbl.Id,
                                                         Type = MappingType.DoubleRegistrableFollower,
                                                         Name = $"{rbl.Name} (Follower)"
                                                     }
                                                  };
                                              }));

            var singleRegistrables = await _registrables.Where(rbl => rbl.EventId == request.EventId
                                                                   && rbl.MaximumSingleSeats != null)
                                                        .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                        .Select(rbl => new QuestionOptionMapping
                                                        {
                                                            Id = rbl.Id,
                                                            Type = MappingType.SingleRegistrable,
                                                            Name = rbl.Name
                                                        })
                                                        .ToListAsync(cancellationToken);
            result.AddRange(singleRegistrables);

            result.Add(new QuestionOptionMapping
            {
                Type = MappingType.Reduction,
                Name = "Reduktion"
            });

            result.ForEach(aqo => aqo.CombinedId = $"{aqo.Id}/{aqo.Type}");

            return result;
        }
    }

    public class QuestionOptionMapping
    {
        public string CombinedId { get; set; } = null!;
        public Guid? Id { get; set; }
        public MappingType? Type { get; set; }
        public string? Name { get; set; }
    }
}