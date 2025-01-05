using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class CheckIfRegistrationHasMultiplePartnersCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
    }

    public class CheckIfRegistrationHasMultiplePartnersCommandHandler(IRepository<Registration> registrations,
                                                                      ChangeTrigger changeTrigger)
        : IRequestHandler<CheckIfRegistrationHasMultiplePartnersCommand>
    {
        public async Task Handle(CheckIfRegistrationHasMultiplePartnersCommand command, CancellationToken cancellationToken)
        {
            var registration = await registrations.Where(reg => reg.EventId == command.EventId
                                                             && reg.Id == command.RegistrationId)
                                                  .Include(reg => reg.Seats_AsLeader!.Where(spot => !spot.IsCancelled && spot.IsPartnerSpot))
                                                  .Include(reg => reg.Seats_AsFollower!.Where(spot => !spot.IsCancelled && spot.IsPartnerSpot))
                                                  .FirstAsync(cancellationToken);

            var partnerCount = Enumerable.Union(registration.Seats_AsLeader!.Select(spot => spot.RegistrationId_Follower),
                                                registration.Seats_AsFollower!.Select(spot => spot.RegistrationId))
                                         .Where(rid => rid != null)
                                         .Distinct()
                                         .Count();
            if (partnerCount > 1)
            {
                // if a registration has multiple partners, treat it as a single registration
                changeTrigger.EnqueueCommand(new UnbindPartnerRegistrationCommand
                                             {
                                                 EventId = command.EventId,
                                                 RegistrationId = registration.Id,
                                                 LeavePartnerSpots = true
                                             });
            }
        }
    }
}