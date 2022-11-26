using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationIsPromotedCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class CheckIfRegistrationIsPromotedCommandHandler : IRequestHandler<CheckIfRegistrationIsPromotedCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReadModelUpdater _readModelUpdater;

    public CheckIfRegistrationIsPromotedCommandHandler(IQueryable<Registration> registrations,
                                                       CommandQueue commandQueue,
                                                       IDateTimeProvider dateTimeProvider,
                                                       ReadModelUpdater readModelUpdater)
    {
        _registrations = registrations;
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _readModelUpdater = readModelUpdater;
    }

    public async Task<Unit> Handle(CheckIfRegistrationIsPromotedCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                               .Include(reg => reg.Seats_AsLeader)
                                               .Include(reg => reg.Seats_AsFollower)
                                               .FirstAsync(cancellationToken);

        var isWaitingListBefore = registration.IsOnWaitingList == true;
        registration.IsOnWaitingList = registration.Seats_AsLeader!.Any(spt => (spt.RegistrableId == command.RegistrationId || spt.RegistrationId_Follower == command.RegistrationId)
                                                                            && spt.IsWaitingList
                                                                            && !spt.IsCancelled)
                                    || registration.Seats_AsFollower!.Any(spt => (spt.RegistrableId == command.RegistrationId || spt.RegistrationId_Follower == command.RegistrationId)
                                                                              && spt.IsWaitingList
                                                                              && !spt.IsCancelled);
        if (isWaitingListBefore && registration.IsOnWaitingList == false)
        {
            // non-core spots are also not on waiting list anymore
            foreach (var spot in registration.Seats_AsLeader!.Where(st => !st.IsCancelled && st.IsWaitingList))
            {
                spot.IsWaitingList = false;
            }

            foreach (var spt in registration.Seats_AsFollower!.Where(st => !st.IsCancelled && st.IsWaitingList))
            {
                spt.IsWaitingList = false;
            }

            registration.AdmittedAt ??= _dateTimeProvider.Now;

            // registration is now accepted, send Mail
            var sendMailCommand = new ComposeAndSendAutoMailCommand
                                  {
                                      EventId = registration.EventId,
                                      MailType = registration.RegistrationId_Partner.HasValue
                                                     ? MailType.PartnerRegistrationMatchedAndAccepted
                                                     : MailType.SingleRegistrationAccepted,
                                      RegistrationId = registration.Id
                                  };
            _commandQueue.EnqueueCommand(sendMailCommand);

            _readModelUpdater.TriggerUpdate<RegistrablesOverviewCalculator>(registration.Id, registration.EventId);
        }

        return Unit.Value;
    }
}