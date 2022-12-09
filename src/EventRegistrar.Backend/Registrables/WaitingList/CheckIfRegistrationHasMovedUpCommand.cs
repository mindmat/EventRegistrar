using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationHasMovedUpCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class CheckIfRegistrationHasMovedUpCommandHandler : IRequestHandler<CheckIfRegistrationHasMovedUpCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReadModelUpdater _readModelUpdater;
    private readonly IEventBus _eventBus;
    private readonly DirtyTagger _dirtyTagger;

    public CheckIfRegistrationHasMovedUpCommandHandler(IQueryable<Registration> registrations,
                                                       CommandQueue commandQueue,
                                                       IDateTimeProvider dateTimeProvider,
                                                       ReadModelUpdater readModelUpdater,
                                                       IEventBus eventBus,
                                                       DirtyTagger dirtyTagger)
    {
        _registrations = registrations;
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _readModelUpdater = readModelUpdater;
        _eventBus = eventBus;
        _dirtyTagger = dirtyTagger;
    }

    public async Task<Unit> Handle(CheckIfRegistrationHasMovedUpCommand command, CancellationToken cancellationToken)
    {
        var dirtyTags = await _dirtyTagger.IsDirty<RegistrationOnWaitingListSegment>(command.RegistrationId);
        var registration = await _registrations.AsTracking()
                                               .Where(reg => reg.Id == command.RegistrationId)
                                               .Include(reg => reg.Seats_AsLeader!.Where(spt => !spt.IsCancelled))
                                               .ThenInclude(spt => spt.Registrable)
                                               .Include(reg => reg.Seats_AsFollower!.Where(spt => !spt.IsCancelled))
                                               .ThenInclude(spt => spt.Registrable)
                                               .FirstAsync(cancellationToken);

        var isWaitingListBefore = registration.IsOnWaitingList == true;
        registration.IsOnWaitingList = registration.Seats_AsLeader!
                                                   .Where(spt => spt.Registrable!.HasWaitingList && spt.Registrable.IsCore)
                                                   .All(spt => spt.IsWaitingList)
                                    || registration.Seats_AsFollower!
                                                   .Where(spt => spt.Registrable!.HasWaitingList && spt.Registrable.IsCore)
                                                   .All(spt => spt.IsWaitingList);
        if (isWaitingListBefore && registration.IsOnWaitingList == false)
        {
            // non-core spots are also not on waiting list anymore
            registration.Seats_AsLeader!
                        .Where(spt => !spt.Registrable!.HasWaitingList && spt.IsWaitingList)
                        .ForEach(spt => spt.IsWaitingList = false);
            registration.Seats_AsFollower!
                        .Where(spt => !spt.Registrable!.HasWaitingList && spt.IsWaitingList)
                        .ForEach(spt => spt.IsWaitingList = false);

            registration.AdmittedAt ??= _dateTimeProvider.Now;

            _eventBus.Publish(new RegistrationMovedUpFromWaitingList { RegistrationId = registration.Id });

            // registration is now accepted, send Mail
            var sendMailCommand = new ComposeAndSendAutoMailCommand
                                  {
                                      EventId = registration.EventId,
                                      MailType = registration.RegistrationId_Partner != null
                                                     ? MailType.PartnerRegistrationMatchedAndAccepted
                                                     : MailType.SingleRegistrationAccepted,
                                      RegistrationId = registration.Id
                                  };
            _commandQueue.EnqueueCommand(sendMailCommand);

            _readModelUpdater.TriggerUpdate<RegistrablesOverviewCalculator>(registration.Id, registration.EventId);
        }

        _dirtyTagger.RemoveDirtyTags(dirtyTags);
        return Unit.Value;
    }
}