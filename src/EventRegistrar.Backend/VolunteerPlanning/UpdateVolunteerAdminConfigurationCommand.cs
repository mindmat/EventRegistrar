using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class UpdateVolunteerAdminConfigurationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<Guid>? RegistrableIds_Volunteer { get; set; }
    public IEnumerable<Guid>? QuestionOptionIds_Volunteer { get; set; }
}

public class UpdateVolunteerAdminConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                             ChangeTrigger changeTrigger)
    : IRequestHandler<UpdateVolunteerAdminConfigurationCommand>
{
    public async Task Handle(UpdateVolunteerAdminConfigurationCommand command, CancellationToken cancellationToken)
    {
        var config = configurationRegistry.GetConfiguration<VolunteerAdminConfiguration>(command.EventId);
        config.RegistrableIds_Volunteer = command.RegistrableIds_Volunteer;
        config.QuestionOptionIds_Volunteer = command.QuestionOptionIds_Volunteer;

        await configurationRegistry.UpdateConfiguration(command.EventId, config);

        changeTrigger.QueryChanged<VolunteerAdminConfigurationQuery>(command.EventId);
    }
}