using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class VolunteerAdminConfigurationQuery : IRequest<VolunteerAdminConfigurationDto>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class VolunteerAdminConfigurationQueryHandler(ConfigurationRegistry configurationRegistry)
    : IRequestHandler<VolunteerAdminConfigurationQuery, VolunteerAdminConfigurationDto>
{
    public Task<VolunteerAdminConfigurationDto> Handle(VolunteerAdminConfigurationQuery query, CancellationToken cancellationToken)
    {
        var configuration = configurationRegistry.GetConfiguration<VolunteerAdminConfiguration>(query.EventId);

        var dto = new VolunteerAdminConfigurationDto
                  {
                      RegistrableIds_Volunteer = configuration.RegistrableIds_Volunteer
                  };

        return Task.FromResult(dto);
    }
}

public class VolunteerAdminConfigurationDto
{
    public IEnumerable<Guid>? RegistrableIds_Volunteer { get; set; }
}