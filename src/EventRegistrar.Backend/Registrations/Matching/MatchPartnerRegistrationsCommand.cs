﻿using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Matching;

public class MatchPartnerRegistrationsCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId1 { get; set; }
    public Guid RegistrationId2 { get; set; }
}

public class MatchPartnerRegistrationsCommandHandler(IRepository<Registration> registrations,
                                                     IRepository<Seat> seats,
                                                     CommandQueue commandQueue,
                                                     ChangeTrigger changeTrigger,
                                                     DirtyTagger dirtyTagger,
                                                     IEventBus eventBus)
    : IRequestHandler<MatchPartnerRegistrationsCommand>
{
    public async Task Handle(MatchPartnerRegistrationsCommand command, CancellationToken cancellationToken)
    {
        if (command.RegistrationId1 == command.RegistrationId2)
        {
            throw new ArgumentException($"Can't assign {command.RegistrationId1} to itself");
        }

        var registration1 = await registrations.Where(reg => reg.EventId == command.EventId
                                                          && reg.Id == command.RegistrationId1)
                                               .Include(reg => reg.Seats_AsLeader!.Where(spt => !spt.IsCancelled && spt.Registrable!.MaximumDoubleSeats != null))
                                               .ThenInclude(seat => seat.Registrable)
                                               .Include(reg => reg.Seats_AsFollower!.Where(spt => !spt.IsCancelled && spt.Registrable!.MaximumDoubleSeats != null))
                                               .ThenInclude(seat => seat.Registrable)
                                               .FirstAsync(cancellationToken);
        var registration2 = await registrations.Where(reg => reg.EventId == command.EventId
                                                          && reg.Id == command.RegistrationId2)
                                               .Include(reg => reg.Seats_AsLeader!.Where(spt => !spt.IsCancelled && spt.Registrable!.MaximumDoubleSeats != null))
                                               .ThenInclude(seat => seat.Registrable)
                                               .Include(reg => reg.Seats_AsFollower!.Where(spt => !spt.IsCancelled && spt.Registrable!.MaximumDoubleSeats != null))
                                               .ThenInclude(seat => seat.Registrable)
                                               .FirstAsync(cancellationToken);

        if (registration1.RegistrationId_Partner != null)
        {
            throw new ArgumentException($"Registration {registration1.Id} is already assigned to a partner");
        }

        if (registration2.RegistrationId_Partner != null)
        {
            throw new ArgumentException($"Registration {registration2.Id} is already assigned to a partner");
        }

        var registrationLeader = registration1;
        var registrationFollower = registration2;
        if (registrationLeader.Seats_AsFollower!.Any())
        {
            registrationLeader = registration2;
            registrationFollower = registration1;
        }

        if (registrationLeader.Seats_AsFollower!.Any())
        {
            throw new ArgumentException($"Unexpected situation: leader registration {registrationLeader.Id} has partner spots as follower");
        }

        if (registrationFollower.Seats_AsLeader!.Any())
        {
            throw new ArgumentException($"Unexpected situation: follower registration {registrationFollower.Id} has partner spots as leader");
        }

        var partnerSpotsOfLeader = registrationLeader.Seats_AsLeader!.ToList();
        var partnerSpotsOfFollower = registrationFollower.Seats_AsFollower!.ToList();

        if (partnerSpotsOfLeader.Any(spt => spt.RegistrationId_Follower != null))
        {
            throw new ArgumentException($"Unexpected situation: leader registration {registrationLeader.Id} has partner spot with a follower set");
        }

        if (partnerSpotsOfFollower.Any(spt => spt.RegistrationId != null))
        {
            throw new ArgumentException($"Unexpected situation: follower registration {registrationFollower.Id} has partner spot with a leader set");
        }

        // ok, everything seems to be fine, let's match
        registrationLeader.RegistrationId_Partner = registrationFollower.Id;
        registrationFollower.RegistrationId_Partner = registrationLeader.Id;

        var registrableIdsToMerge = partnerSpotsOfLeader.Select(spt => spt.RegistrableId)
                                                        .Intersect(partnerSpotsOfFollower.Select(spt => spt.RegistrableId))
                                                        .Distinct()
                                                        .ToList();

        var isWaitingList = false;
        foreach (var registrableId in registrableIdsToMerge)
        {
            var leaderSpot = partnerSpotsOfLeader.Single(spt => spt.RegistrableId == registrableId);
            var followerSpot = partnerSpotsOfFollower.Single(spt => spt.RegistrableId == registrableId);
            Seat mergedSpot;
            Seat spotToCancel;
            if (leaderSpot.FirstPartnerJoined < followerSpot.FirstPartnerJoined || (!leaderSpot.IsWaitingList && followerSpot.IsWaitingList))
            {
                mergedSpot = leaderSpot;
                mergedSpot.RegistrationId_Follower = registrationFollower.Id;
                spotToCancel = followerSpot;
            }
            else
            {
                mergedSpot = followerSpot;
                mergedSpot.RegistrationId = registrationLeader.Id;
                spotToCancel = leaderSpot;
            }

            spotToCancel.IsCancelled = true;
            mergedSpot.IsPartnerSpot = true;

            await seats.InsertOrUpdateEntity(mergedSpot, cancellationToken);
            await seats.InsertOrUpdateEntity(spotToCancel, cancellationToken);
            isWaitingList |= mergedSpot.IsWaitingList;
        }

        // update waiting list
        dirtyTagger.UpdateSegment<RegistrationPriceAndWaitingListSegment>(registration1.Id);
        dirtyTagger.UpdateSegment<RegistrationPriceAndWaitingListSegment>(registration2.Id);

        var mailType = isWaitingList
                           ? MailType.PartnerRegistrationMatchedOnWaitingList
                           : MailType.PartnerRegistrationMatchedAndAccepted;
        commandQueue.EnqueueCommand(new ComposeAndSendAutoMailCommand
                                    {
                                        EventId = command.EventId,
                                        RegistrationId = registrationLeader.Id,
                                        MailType = mailType
                                    });

        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration1.Id, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration2.Id, command.EventId);
        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(RegistrationsWithUnmatchedPartnerQuery)
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(PotentialPartnersQuery),
                             RowId = registration1.Id
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(PotentialPartnersQuery),
                             RowId = registration2.Id
                         });
    }
}