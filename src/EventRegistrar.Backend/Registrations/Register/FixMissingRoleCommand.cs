using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.Raw;

namespace EventRegistrar.Backend.Registrations.Register;

public class FixMissingRoleCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RawRegistrationId { get; set; }
    public Role Role { get; set; }
}

public class FixMissingRoleCommandHandler(IRepository<RawRegistration> rawRegistrations,
                                          IQueryable<Event> events,
                                          ChangeTrigger changeTrigger) : IRequestHandler<FixMissingRoleCommand>
{
    public async Task Handle(FixMissingRoleCommand command, CancellationToken cancellationToken)
    {
        var eventAcronym = await events.Where(evt => evt.Id == command.EventId)
                                       .Select(evt => evt.Acronym)
                                       .FirstAsync(cancellationToken);

        var rawRegistration = await rawRegistrations.AsTracking()
                                                    .FirstAsync(rrg => rrg.EventAcronym == eventAcronym
                                                                    && rrg.Id == command.RawRegistrationId, cancellationToken);
        if (rawRegistration is { RoleMissing: true, Processed: null }
         && Enum.IsDefined(command.Role))
        {
            rawRegistration.RoleOverride = command.Role;
            changeTrigger.EnqueueCommand(new ProcessRawRegistrationCommand
                                         {
                                             RawRegistrationId = rawRegistration.Id
                                         });
        }
    }
}