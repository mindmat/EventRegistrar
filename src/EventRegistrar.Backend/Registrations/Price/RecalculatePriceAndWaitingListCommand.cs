﻿using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceAndWaitingListCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class RecalculatePriceAndWaitingListCommandHandler : AsyncRequestHandler<RecalculatePriceAndWaitingListCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly IEventBus _eventBus;
    private readonly PriceCalculator _priceCalculator;
    private readonly DirtyTagger _dirtyTagger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ChangeTrigger _changeTrigger;
    private readonly CommandQueue _commandQueue;

    public RecalculatePriceAndWaitingListCommandHandler(IRepository<Registration> registrations,
                                                        IEventBus eventBus,
                                                        PriceCalculator priceCalculator,
                                                        DirtyTagger dirtyTagger,
                                                        IDateTimeProvider dateTimeProvider,
                                                        ChangeTrigger changeTrigger,
                                                        CommandQueue commandQueue)
    {
        _registrations = registrations;
        _eventBus = eventBus;
        _priceCalculator = priceCalculator;
        _dirtyTagger = dirtyTagger;
        _dateTimeProvider = dateTimeProvider;
        _changeTrigger = changeTrigger;
        _commandQueue = commandQueue;
    }

    protected override async Task Handle(RecalculatePriceAndWaitingListCommand command, CancellationToken cancellationToken)
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

        var (newOriginal, newAdmitted, newAdmittedAndReduced, _, packagesAdmitted, isOnWaitingList, _) = await _priceCalculator.CalculatePrice(registration.Id, cancellationToken);
        var packageIds_admitted = packagesAdmitted.Select(pkg => pkg.Id)
                                                  .Where(id => id != null)
                                                  .Select(id => id!.Value)
                                                  .OrderBy(id => id)
                                                  .MergeKeys();
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

        // update admitted package(s)
        if (registration.PricePackageIds_Admitted != packageIds_admitted)
        {
            registration.PricePackageIds_Admitted = packageIds_admitted;

            _eventBus.Publish(new QueryChanged
                              {
                                  EventId = registration.EventId,
                                  QueryName = nameof(PricePackageOverview)
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

            _changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, registration.EventId);
            _changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        }

        _dirtyTagger.RemoveDirtyTags(dirtyTags);
    }
}

public class RecalculatePriceAndWaitingList : IEventToCommandTranslation<RegistrationCancelled>
{
    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }
}