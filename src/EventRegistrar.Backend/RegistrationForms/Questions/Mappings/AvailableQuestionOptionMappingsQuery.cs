using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class AvailableQuestionOptionMappingsQuery : IRequest<IEnumerable<AvailableQuestionOptionMapping>>
{
    public Guid EventId { get; set; }
}

public class AvailableQuestionOptionMappingsQueryHandler(IQueryable<Registrable> registrables) : IRequestHandler<AvailableQuestionOptionMappingsQuery, IEnumerable<AvailableQuestionOptionMapping>>
{
    public async Task<IEnumerable<AvailableQuestionOptionMapping>> Handle(AvailableQuestionOptionMappingsQuery query,
                                                                          CancellationToken cancellationToken)
    {
        var result = new List<AvailableQuestionOptionMapping>();
        var doubleRegistrables = await registrables.Where(rbl => rbl.EventId == query.EventId
                                                              && rbl.MaximumDoubleSeats != null)
                                                   .Select(rbl => new
                                                                  {
                                                                      rbl.Id,
                                                                      rbl.DisplayName,
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
                                                             Type = MappingType.PartnerRegistrableLeader,
                                                             Id = rbl.Id,
                                                             Name = $"{rbl.DisplayName} ({Properties.Resources.Leader})"
                                                         },
                                                         new AvailableQuestionOptionMapping
                                                         {
                                                             Type = MappingType.PartnerRegistrableFollower,
                                                             Id = rbl.Id,
                                                             Name = $"{rbl.DisplayName} ({Properties.Resources.Follower})"
                                                         },
                                                         new AvailableQuestionOptionMapping
                                                         {
                                                             Type = MappingType.PartnerRegistrable,
                                                             Id = rbl.Id,
                                                             Name = rbl.DisplayName
                                                         }
                                                     };
                                          }));

        var singleRegistrables = await registrables.Where(rbl => rbl.EventId == query.EventId
                                                              && rbl.MaximumDoubleSeats == null)
                                                   .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                   .Select(rbl => new AvailableQuestionOptionMapping
                                                                  {
                                                                      Type = MappingType.SingleRegistrable,
                                                                      Id = rbl.Id,
                                                                      Name = rbl.DisplayName
                                                                  })
                                                   .ToListAsync(cancellationToken);
        result.AddRange(singleRegistrables);

        // Languages
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.Language,
                       Name = $"{Properties.Resources.Language}: {Properties.Resources.German}",
                       Language = Language.German
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

        // Hosting
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.HostingOffer,
                       Name = Properties.Resources.MappingType_HostingOffer
                   });
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.HostingRequest,
                       Name = Properties.Resources.MappingType_HostingRequest
                   });
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.HostingRequest_ShareOkWithRandom,
                       Name = Properties.Resources.MappingType_HostingRequest_ShareOkWithRandom
                   });
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.HostingRequest_ShareOkWithPartner,
                       Name = Properties.Resources.MappingType_HostingRequest_ShareOkWithPartner
                   });
        result.Add(new AvailableQuestionOptionMapping
                   {
                       Type = MappingType.HostingRequest_TravelByCar,
                       Name = Properties.Resources.MappingType_HostingRequest_TravelByCar
                   });


        result.ForEach(aqo => aqo.CombinedId = new CombinedMappingId(aqo.Type, aqo.Id, aqo.Language).ToString());

        return result;
    }
}

public class AvailableQuestionOptionMapping
{
    public MappingType Type { get; set; }
    public Guid? Id { get; set; }
    public string? Language { get; set; }
    public string CombinedId { get; set; } = null!;
    public string? Name { get; set; }
}