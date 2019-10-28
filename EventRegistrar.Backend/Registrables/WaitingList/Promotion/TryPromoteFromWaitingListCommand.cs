﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrableId { get; set; }
    }

    public class TryPromoteFromWaitingListCommandHandler : IRequestHandler<TryPromoteFromWaitingListCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly ImbalanceManager _imbalanceManager;
        private readonly ILogger _log;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IRepository<Seat> _spots;

        public TryPromoteFromWaitingListCommandHandler(IQueryable<Registrable> registrables,
                                                       IRepository<Seat> spots,
                                                       ImbalanceManager imbalanceManager,
                                                       IEventBus eventBus,
                                                       ILogger log)
        {
            _registrables = registrables;
            _spots = spots;
            _imbalanceManager = imbalanceManager;
            _eventBus = eventBus;
            _log = log;
        }

        public async Task<Unit> Handle(TryPromoteFromWaitingListCommand command, CancellationToken cancellationToken)
        {
            var registrableToCheck = await _registrables.Where(rbl => rbl.Id == command.RegistrableId
                                                                   && rbl.EventId == command.EventId)
                                                        .Include(rbl => rbl.Seats)
                                                        .FirstOrDefaultAsync(cancellationToken);
            var spots = registrableToCheck.Seats.Where(spt => !spt.IsCancelled).ToList();
            if (registrableToCheck.MaximumSingleSeats.HasValue)
            {
                var acceptedSpotCount = spots.Count(spt => !spt.IsWaitingList);
                var spotsAvailable = registrableToCheck.MaximumSingleSeats.Value - acceptedSpotCount;
                if (spotsAvailable > 0)
                {
                    var spotsToPromote = spots.Where(spt => spt.IsWaitingList)
                                              .OrderBy(spt => spt.FirstPartnerJoined)
                                              .Take(spotsAvailable);
                    foreach (var spot in spotsToPromote)
                    {
                        await PromoteSpotFromWaitingList(spot);
                    }
                }
            }
            else if (registrableToCheck.MaximumDoubleSeats.HasValue)
            {
                var singleSpotsAccepted = new Queue<Seat>(spots.Where(spt => !spt.IsWaitingList
                                                                          && !spt.IsPartnerSpot
                                                                          && (spt.RegistrationId == null
                                                                           || spt.RegistrationId_Follower == null)));

                var waitinglist = spots.Where(spt => spt.IsWaitingList)
                                       .OrderBy(spt => spt.FirstPartnerJoined)
                                       .ToList();
                // fill the gaps
                while (singleSpotsAccepted.Any())
                {
                    var spotToComplement = singleSpotsAccepted.Dequeue();
                    await ComplementSeatFromWaitingList(spotToComplement, waitinglist);
                }

                while (registrableToCheck.MaximumDoubleSeats.Value - spots.Count(spt => !spt.IsWaitingList) > 0)
                {
                    var nextSpotOnWaitingList = waitinglist.FirstOrDefault();
                    if (nextSpotOnWaitingList == null)
                    {
                        break;
                    }

                    if (nextSpotOnWaitingList.IsPartnerSpot)
                    {
                        await PromoteSpotFromWaitingList(nextSpotOnWaitingList, waitinglist);
                        continue;
                    }

                    // next spot is single spot. decide: match with other role or take next partner spot?
                    var firstSingleRole = nextSpotOnWaitingList.GetSingleRole();
                    var otherRole = firstSingleRole.GetOtherRole();
                    var nextSingleInOtherRole = waitinglist.FirstOrDefault(spt => otherRole == Role.Leader
                                                                                  ? spt.IsSingleLeaderSpot()
                                                                                  : spt.IsSingleFollowerSpot());
                    var nextPartnerRegistration = waitinglist.FirstOrDefault(spt => spt.IsPartnerSpot);
                    if (nextSingleInOtherRole == null)
                    {
                        if (nextPartnerRegistration == null)
                        {
                            if (!_imbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrableToCheck.MaximumDoubleSeats.Value,
                                                                                            registrableToCheck.MaximumAllowedImbalance ?? 0,
                                                                                            spots,
                                                                                            firstSingleRole))
                            {
                                // no promotion due to imbalance
                                waitinglist.Remove(nextSpotOnWaitingList);
                                continue;
                            }

                            await PromoteSpotFromWaitingList(nextSpotOnWaitingList, waitinglist);
                        }
                        else
                        {
                            await PromoteSpotFromWaitingList(nextPartnerRegistration, waitinglist);
                        }
                    }
                    else
                    {
                        if (nextPartnerRegistration != null)
                        {
                            // who should be first: the two singles or partner?
                            var averageOfSingles = GetAverage(nextSpotOnWaitingList.FirstPartnerJoined,
                                                              nextSingleInOtherRole.FirstPartnerJoined);
                            if (averageOfSingles > nextPartnerRegistration.FirstPartnerJoined)
                            {
                                // partner registration wins
                                await PromoteSpotFromWaitingList(nextPartnerRegistration, waitinglist);
                                continue;
                            }
                        }

                        nextSpotOnWaitingList.MergeSingleSpots(nextSingleInOtherRole);
                        await PromoteSpotFromWaitingList(nextSpotOnWaitingList);
                        await _spots.InsertOrUpdateEntity(nextSingleInOtherRole, cancellationToken);
                        waitinglist.Remove(nextSpotOnWaitingList);
                        waitinglist.Remove(nextSingleInOtherRole);
                    }
                }
            }

            return Unit.Value;
        }

        private static DateTime GetAverage(DateTime dateTime1, DateTime dateTime2)
        {
            return new DateTime((dateTime1.Ticks + dateTime2.Ticks) / 2);
        }

        private async Task<bool> ComplementSeatFromWaitingList(Seat seatToComplement, List<Seat> waitinglist)
        {
            if (seatToComplement.IsSingleLeaderSpot())
            {
                // follower spot available, find follower on waiting list
                var nextFollowerOnWaitingList = waitinglist.FirstOrDefault(spt => spt.IsSingleFollowerSpot());
                if (nextFollowerOnWaitingList?.RegistrationId_Follower == null)
                {
                    return false;
                }
                if (nextFollowerOnWaitingList.RegistrationId != null)
                {
                    throw new Exception("Unexpected situation: single spot on waiting list has both leader and follower set");
                }

                seatToComplement.RegistrationId_Follower = nextFollowerOnWaitingList.RegistrationId_Follower;
                nextFollowerOnWaitingList.IsCancelled = true;
                waitinglist.Remove(nextFollowerOnWaitingList);

                await _spots.InsertOrUpdateEntity(seatToComplement);
                await _spots.InsertOrUpdateEntity(nextFollowerOnWaitingList);
                _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                {
                    Id = Guid.NewGuid(),
                    RegistrableId = seatToComplement.RegistrableId,
                    RegistrationId = nextFollowerOnWaitingList.RegistrationId_Follower.Value
                });
                return true;
            }
            if (seatToComplement.IsSingleFollowerSpot())
            {
                // follower spot available, find follower on waiting list
                var nextLeaderOnWaitingList = waitinglist.FirstOrDefault(spt => spt.IsSingleLeaderSpot());
                if (nextLeaderOnWaitingList?.RegistrationId == null)
                {
                    return false;
                }
                if (nextLeaderOnWaitingList.RegistrationId_Follower != null)
                {
                    throw new Exception("Unexpected situation: single spot on waiting list has both leader and follower set");
                }

                seatToComplement.RegistrationId = nextLeaderOnWaitingList.RegistrationId;
                nextLeaderOnWaitingList.IsCancelled = true;
                waitinglist.Remove(nextLeaderOnWaitingList);

                await _spots.InsertOrUpdateEntity(seatToComplement);
                await _spots.InsertOrUpdateEntity(nextLeaderOnWaitingList);
                _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                {
                    Id = Guid.NewGuid(),
                    RegistrableId = seatToComplement.RegistrableId,
                    RegistrationId = nextLeaderOnWaitingList.RegistrationId.Value
                });
                return true;
            }

            return false;
        }

        private async Task PromoteSpotFromWaitingList(Seat spot, List<Seat> waitinglist = null)
        {
            var registrationId = spot.RegistrationId ?? spot.RegistrationId_Follower;
            if (registrationId == null)
            {
                _log.LogWarning("Unexpected situation: Spot {0} has neither leader nor follower set", spot.Id);
                return;
            }

            spot.IsWaitingList = false;
            await _spots.InsertOrUpdateEntity(spot);

            waitinglist?.Remove(spot);

            if (spot.IsPartnerSpot)
            {
                _eventBus.Publish(new PartnerSpotPromotedFromWaitingList
                {
                    Id = Guid.NewGuid(),
                    RegistrableId = spot.RegistrableId,
                    RegistrationId = spot.RegistrationId,
                    RegistrationId_Follower = spot.RegistrationId_Follower
                });
            }
            else
            {
                if (spot.RegistrationId.HasValue)
                {
                    _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                    {
                        Id = Guid.NewGuid(),
                        RegistrableId = spot.RegistrableId,
                        RegistrationId = spot.RegistrationId.Value
                    });
                }

                if (spot.RegistrationId_Follower.HasValue)
                {
                    _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                    {
                        Id = Guid.NewGuid(),
                        RegistrableId = spot.RegistrableId,
                        RegistrationId = spot.RegistrationId_Follower.Value
                    });
                }
            }
        }
    }
}