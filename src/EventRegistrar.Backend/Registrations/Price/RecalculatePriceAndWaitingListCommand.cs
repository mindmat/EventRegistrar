using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceAndWaitingListCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class RecalculatePriceAndWaitingListCommandHandler(IRepository<Registration> registrations,
                                                          IEventBus eventBus,
                                                          PriceCalculator priceCalculator,
                                                          DirtyTagger dirtyTagger,
                                                          IDateTimeProvider dateTimeProvider,
                                                          ChangeTrigger changeTrigger,
                                                          CommandQueue commandQueue)
    : IRequestHandler<RecalculatePriceAndWaitingListCommand>
{
    public async Task Handle(RecalculatePriceAndWaitingListCommand command, CancellationToken cancellationToken)
    {
        var dirtyTags = await dirtyTagger.IsDirty<RegistrationPriceAndWaitingListSegment>(command.RegistrationId);
        var registration = await registrations.AsTracking()
                                              .Include(reg => reg.Seats_AsLeader!)
                                              .ThenInclude(spt => spt.Registrable)
                                              .Include(reg => reg.Seats_AsFollower!)
                                              .ThenInclude(spt => spt.Registrable)
                                              .FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
        var oldOriginal = registration.Price_Original;
        var oldAdmitted = registration.Price_Admitted;
        var oldAdmittedAndReduced = registration.Price_AdmittedAndReduced;

        var (newOriginal, newAdmitted, newAdmittedAndReduced, _, packagesAdmitted, isOnWaitingList, _) = await priceCalculator.CalculatePrice(registration.Id, cancellationToken);
        var packageIds_admitted = packagesAdmitted.Select(pkg => pkg.Id)
                                                  .Where(id => id != null)
                                                  .Select(id => id!.Value)
                                                  .OrderBy(id => id)
                                                  .ToList();
        // update price
        if (oldOriginal != newOriginal
         || oldAdmitted != newAdmitted
         || oldAdmittedAndReduced != newAdmittedAndReduced)
        {
            registration.Price_Original = newOriginal;
            registration.Price_Admitted = newAdmitted;
            registration.Price_AdmittedAndReduced = newAdmittedAndReduced;

            eventBus.Publish(new PriceChanged
                             {
                                 EventId = registration.EventId,
                                 RegistrationId = registration.Id,
                                 OldPrice = oldAdmittedAndReduced,
                                 NewPrice = newAdmittedAndReduced
                             });
        }

        // update admitted package(s)
        if (registration.PricePackageIds_Admitted?.SequenceEqual(packageIds_admitted) != true)
        {
            registration.PricePackageIds_Admitted = packageIds_admitted;

            eventBus.Publish(new QueryChanged
                             {
                                 EventId = registration.EventId,
                                 QueryName = nameof(PricePackageOverview)
                             });
        }

        // update waiting list
        if (registration.IsOnWaitingList != isOnWaitingList)
        {
            registration.IsOnWaitingList = isOnWaitingList;
            registration.AdmittedAt ??= dateTimeProvider.Now;

            eventBus.Publish(new RegistrationMovedUpFromWaitingList { RegistrationId = registration.Id });

            // registration is now accepted, send Mail
            var sendMailCommand = new ComposeAndSendAutoMailCommand
                                  {
                                      EventId = registration.EventId,
                                      MailType = registration.RegistrationId_Partner != null
                                                     ? MailType.PartnerRegistrationMatchedAndAccepted
                                                     : MailType.SingleRegistrationAccepted,
                                      RegistrationId = registration.Id
                                  };
            commandQueue.EnqueueCommand(sendMailCommand);

            changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, registration.EventId);
            changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        }

        dirtyTagger.RemoveDirtyTags(dirtyTags);
    }
}

public class RecalculatePriceAndWaitingList : IEventToCommandTranslation<RegistrationCancelled>
{
    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }
}