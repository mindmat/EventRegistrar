using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class VolunteerAdminConfiguration : IConfigurationItem
{
    public IEnumerable<Guid>? RegistrableIds_Volunteer { get; set; }
}

public class DefaultVolunteerAdminConfiguration : VolunteerAdminConfiguration, IDefaultConfigurationItem
{
    public DefaultVolunteerAdminConfiguration()
    {
        RegistrableIds_Volunteer = [];
    }
}