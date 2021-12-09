using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class AvailableQuestionOptionMappingsQuery : IRequest<IEnumerable<AvailableQuestionOptionMapping>>
{
    public Guid EventId { get; set; }
}

public class AvailableQuestionOptionMappingsQueryHandler : IRequestHandler<AvailableQuestionOptionMappingsQuery,
    IEnumerable<AvailableQuestionOptionMapping>>
{
    private readonly IQueryable<Registrable> _registrables;

    public AvailableQuestionOptionMappingsQueryHandler(IQueryable<Registrable> registrables)
    {
        _registrables = registrables;
    }

    public async Task<IEnumerable<AvailableQuestionOptionMapping>> Handle(
        AvailableQuestionOptionMappingsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<AvailableQuestionOptionMapping>();
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
                                                         new AvailableQuestionOptionMapping
                                                         {
                                                             Id = rbl.Id,
                                                             Type = MappingType.PartnerRegistrableLeader,
                                                             Name = $"{rbl.Name} ({Properties.Resources.Leader})"
                                                         },
                                                         new AvailableQuestionOptionMapping
                                                         {
                                                             Id = rbl.Id,
                                                             Type = MappingType.PartnerRegistrableFollower,
                                                             Name = $"{rbl.Name} ({Properties.Resources.Follower})"
                                                         },
                                                         new AvailableQuestionOptionMapping
                                                         {
                                                             Id = rbl.Id,
                                                             Type = MappingType.PartnerRegistrable,
                                                             Name = rbl.Name
                                                         }
                                                     };
                                          }));

        var singleRegistrables = await _registrables.Where(rbl => rbl.EventId == request.EventId
                                                               && rbl.MaximumDoubleSeats == null)
                                                    .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                    .Select(rbl => new AvailableQuestionOptionMapping
                                                                   {
                                                                       Id = rbl.Id,
                                                                       Type = MappingType.SingleRegistrable,
                                                                       Name = rbl.Name
                                                                   })
                                                    .ToListAsync(cancellationToken);
        result.AddRange(singleRegistrables);

        // Reduction
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.Reduction,
                       Name = Properties.Resources.Reduction
                   });

        // Languages
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.Language,
                       Name = $"{Properties.Resources.Language}: {Properties.Resources.German}",
                       Language = Language.Deutsch
                   });
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.Language,
                       Name = $"{Properties.Resources.Language}: {Properties.Resources.English}",
                       Language = Language.English
                   });

        // Roles
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.RoleLeader,
                       Name = $"{Properties.Resources.Role}: {Properties.Resources.Leader}"
                   });
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.RoleFollower,
                       Name = $"{Properties.Resources.Role}: {Properties.Resources.Follower}"
                   });

        result.ForEach(aqo => aqo.CombinedId = $"{aqo.Id}/{aqo.Type}/{aqo.Language}");

        return result;
    }
}

public class AvailableQuestionOptionMapping
{
    public string CombinedId { get; set; } = null!;
    public Guid? Id { get; set; }
    public MappingType? Type { get; set; }
    public string? Language { get; set; }
    public string? Name { get; set; }
}