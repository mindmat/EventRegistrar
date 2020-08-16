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
    public class AvailableQuestionOptionMappingsQuery : IRequest<IEnumerable<QuestionOptionMappingDisplayItem>>
    {
        public Guid EventId { get; set; }
    }

    public class AvailableQuestionOptionMappingsQueryHandler : IRequestHandler<AvailableQuestionOptionMappingsQuery, IEnumerable<QuestionOptionMappingDisplayItem>>
    {
        private readonly IQueryable<Registrable> _registrables;

        public AvailableQuestionOptionMappingsQueryHandler(IQueryable<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<IEnumerable<QuestionOptionMappingDisplayItem>> Handle(AvailableQuestionOptionMappingsQuery request, CancellationToken cancellationToken)
        {
            var result = new List<QuestionOptionMappingDisplayItem>();
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
                                                     new QuestionOptionMappingDisplayItem
                                                     {
                                                         Id = rbl.Id,
                                                         Type = MappingType.DoubleRegistrableLeader,
                                                         Name = $"{rbl.Name} ({Properties.Resources.Leader})"
                                                     },
                                                     new QuestionOptionMappingDisplayItem
                                                     {
                                                         Id = rbl.Id,
                                                         Type = MappingType.DoubleRegistrableFollower,
                                                         Name = $"{rbl.Name} ({Properties.Resources.Follower})"
                                                     }
                                                  };
                                              }));

            var singleRegistrables = await _registrables.Where(rbl => rbl.EventId == request.EventId
                                                                   && rbl.MaximumDoubleSeats == null)
                                                        .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                        .Select(rbl => new QuestionOptionMappingDisplayItem
                                                        {
                                                            Id = rbl.Id,
                                                            Type = MappingType.SingleRegistrable,
                                                            Name = rbl.Name
                                                        })
                                                        .ToListAsync(cancellationToken);
            result.AddRange(singleRegistrables);

            // Reduction
            result.Add(new QuestionOptionMappingDisplayItem
            {
                Type = MappingType.Reduction,
                Name = Properties.Resources.Reduction
            });

            // Languages
            result.Add(new QuestionOptionMappingDisplayItem
            {
                Type = MappingType.Language,
                Name = $"{Properties.Resources.Language}: {Properties.Resources.German}",
                Language = "de"
            });

            result.Add(new QuestionOptionMappingDisplayItem
            {
                Type = MappingType.Language,
                Name = $"{Properties.Resources.Language}: {Properties.Resources.English}",
                Language = "en"
            });

            // Roles
            result.Add(new QuestionOptionMappingDisplayItem
            {
                Type = MappingType.RoleLeader,
                Name = $"{Properties.Resources.Role}: {Properties.Resources.Leader}"
            });
            result.Add(new QuestionOptionMappingDisplayItem
            {
                Type = MappingType.RoleFollower,
                Name = $"{Properties.Resources.Role}: {Properties.Resources.Follower}"
            });

            result.ForEach(aqo => aqo.CombinedId = $"{aqo.Id}/{aqo.Type}/{aqo.Language}");

            return result;
        }
    }

    public class QuestionOptionMappingDisplayItem
    {
        public string CombinedId { get; set; } = null!;
        public Guid? Id { get; set; }
        public MappingType? Type { get; set; }
        public string? Language { get; set; }
        public string? Name { get; set; }
    }
}