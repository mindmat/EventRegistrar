using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.WaitingList;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceAndWaitingListCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class RecalculatePriceAndWaitingListCommandHandler : IRequestHandler<RecalculatePriceAndWaitingListCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly IEventBus _eventBus;
    private readonly PriceCalculator _priceCalculator;
    private readonly DirtyTagger _dirtyTagger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReadModelUpdater _readModelUpdater;
    private readonly CommandQueue _commandQueue;

    public RecalculatePriceAndWaitingListCommandHandler(IRepository<Registration> registrations,
                                                        IEventBus eventBus,
                                                        PriceCalculator priceCalculator,
                                                        DirtyTagger dirtyTagger,
                                                        IDateTimeProvider dateTimeProvider,
                                                        ReadModelUpdater readModelUpdater,
                                                        CommandQueue commandQueue)
    {
        _registrations = registrations;
        _eventBus = eventBus;
        _priceCalculator = priceCalculator;
        _dirtyTagger = dirtyTagger;
        _dateTimeProvider = dateTimeProvider;
        _readModelUpdater = readModelUpdater;
        _commandQueue = commandQueue;
    }

    public async Task<Unit> Handle(RecalculatePriceAndWaitingListCommand command, CancellationToken cancellationToken)
    {
        var dirtyTags = await _dirtyTagger.IsDirty<RegistrationPriceAndWaitingListSegment>(command.RegistrationId);
        var registration = await _registrations.AsTracking()
                                               .Include(reg => reg.Seats_AsLeader!)
                                               .ThenInclude(spt => spt.Registrable)
                                               .Include(reg => reg.Seats_AsFollower!)
                                               .ThenInclude(spt => spt.Registrable)
                                               .FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
        var oldOriginal = registration.Price_Original;
        var oldAdmitted = registration.Price_Admitted;
        var oldAdmittedAndReduced = registration.Price_AdmittedAndReduced;

        var (newOriginal, newAdmitted, newAdmittedAndReduced, _, _, isOnWaitingList) = await _priceCalculator.CalculatePrice(registration.Id, cancellationToken);

        // update price
        if (oldOriginal != newOriginal
         || oldAdmitted != newAdmitted
         || oldAdmittedAndReduced != newAdmittedAndReduced)
        {
            registration.Price_Original = newOriginal;
            registration.Price_Admitted = newAdmitted;
            registration.Price_AdmittedAndReduced = newAdmittedAndReduced;
            _eventBus.Publish(new PriceChanged
                              {
                                  EventId = registration.EventId,
                                  RegistrationId = registration.Id,
                                  OldPrice = oldAdmittedAndReduced,
                                  NewPrice = newAdmittedAndReduced
                              });
        }

        // update waiting list
        if (registration.IsOnWaitingList != isOnWaitingList)
        {
            registration.IsOnWaitingList = isOnWaitingList;

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