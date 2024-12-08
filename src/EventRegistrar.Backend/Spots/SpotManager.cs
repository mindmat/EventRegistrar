using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Register;

namespace EventRegistrar.Backend.Spots;

public class SpotManager(IRepository<Seat> _spots,
                         ImbalanceManager imbalanceManager,
                         ILogger logger,
                         IQueryable<Registration> registrations,
                         IQueryable<Registrable> registrables,
                         IEventBus eventBus,
                         IDateTimeProvider dateTimeProvider)
{
    public async Task<Seat?> ReservePartnerSpot(Guid eventId,
                                                Registrable registrable,
                                                Guid registrationId_Leader,
                                                Guid registrationId_Follower,
                                                bool initialProcessing)
    {
        var seats = registrable.Spots!.Where(st => !st.IsCancelled).ToList();
        if (registrable.MaximumSingleSeats.HasValue)
        {
            throw new InvalidOperationException("Unexpected: Attempt to reserve single spot as partner spot");
        }

        var seat = new Seat
                   {
                       Id = Guid.NewGuid(),
                       RegistrationId = registrationId_Leader,
                       RegistrationId_Follower = registrationId_Follower,
                       RegistrableId = registrable.Id,
                       IsPartnerSpot = true,
                       FirstPartnerJoined = dateTimeProvider.Now
                   };
        if (registrable.MaximumDoubleSeats != null)
        {
            var waitingListForPartnerRegistrations = seats.Any(st => st.IsWaitingList && st.IsPartnerSpot);
            var seatAvailable = !waitingListForPartnerRegistrations && seats.Count < registrable.MaximumDoubleSeats.Value;
            if (!seatAvailable && !registrable.HasWaitingList)
                // no spot available, no waiting list
            {
                return null;
            }

            seat.IsWaitingList = !seatAvailable;
        }

        _spots.InsertObjectTree(seat);
        eventBus.Publish(new SpotAdded
                         {
                             Id = Guid.NewGuid(),
                             EventId = eventId,
                             RegistrableId = registrable.Id,
                             Registrable = registrable.DisplayName,
                             RegistrationId = registrationId_Leader,
                             IsInitialProcessing = initialProcessing,
                             IsWaitingList = seat.IsWaitingList
                         });
        eventBus.Publish(new SpotAdded
                         {
                             Id = Guid.NewGuid(),
                             EventId = eventId,
                             RegistrableId = registrable.Id,
                             Registrable = registrable.DisplayName,
                             RegistrationId = registrationId_Follower,
                             IsInitialProcessing = initialProcessing,
                             IsWaitingList = seat.IsWaitingList
                         });
        return seat;
    }

    public async Task<Seat?> ReserveSinglePartOfPartnerSpot(Guid eventId,
                                                            Guid registrableId,
                                                            Guid registrationId,
                                                            RegistrationIdentification ourIdentification,
                                                            string? partnerText,
                                                            Guid? registrationId_Partner,
                                                            Role? ourRole,
                                                            bool initialProcessing)
    {
        Seat spot;
        var registrable = await registrables.Where(rbl => rbl.Id == registrableId)
                                            .Include(rbl => rbl.Spots)
                                            .FirstAsync();
        var spots = registrable.Spots!.Where(st => !st.IsCancelled)
                               .ToList();
        if (registrable.MaximumSingleSeats != null)
        {
            var waitingList = spots.Any(spot => spot.IsWaitingList);
            var spotAvailable = !waitingList && spots.Count < registrable.MaximumSingleSeats.Value;
            logger.LogInformation($"Registrable {registrable.DisplayName}, spot count {spots.Count}, MaximumSingleSeats {registrable.MaximumSingleSeats}, seat available {spotAvailable}");
            if (!spotAvailable && !registrable.HasWaitingList)
            {
                return null;
            }

            spot = new Seat
                   {
                       FirstPartnerJoined = dateTimeProvider.Now,
                       RegistrationId = registrationId,
                       RegistrableId = registrable.Id,
                       IsWaitingList = !spotAvailable
                   };
        }
        else if (registrable.MaximumDoubleSeats != null)
        {
            var isPartnerRegistration = !string.IsNullOrEmpty(partnerText) || registrationId_Partner != null;
            var waitingList = spots.Where(st => st.IsWaitingList).ToList();
            if (isPartnerRegistration)
            {
                // complement existing partner seat
                var existingPartnerSeat = await FindPartnerSpot(eventId,
                                                                ourIdentification,
                                                                partnerText,
                                                                registrationId_Partner,
                                                                ourRole,
                                                                spots);

                if (existingPartnerSeat != null)
                {
                    ComplementExistingSeat(registrationId, ourRole, existingPartnerSeat);
                    eventBus.Publish(new SpotAdded
                                     {
                                         Id = Guid.NewGuid(),
                                         EventId = eventId,
                                         RegistrableId = registrable.Id,
                                         Registrable = registrable.DisplayName,
                                         RegistrationId = registrationId,
                                         IsInitialProcessing = initialProcessing,
                                         IsWaitingList = existingPartnerSeat.IsWaitingList
                                     });
                    return existingPartnerSeat;
                }

                // create new partner seat
                var waitingListForPartnerRegistrations = waitingList.Any(st => !string.IsNullOrEmpty(st.PartnerEmail));
                var spotsAvailable = !waitingListForPartnerRegistrations
                                  && spots.Count < registrable.MaximumDoubleSeats.Value;
                if (!spotsAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }

                spot = new Seat
                       {
                           FirstPartnerJoined = dateTimeProvider.Now,
                           PartnerEmail = partnerText?.ToLowerInvariant(),
                           RegistrableId = registrable.Id,
                           IsWaitingList = ourRole != null,
                           IsPartnerSpot = true
                       };
                if (ourRole == Role.Follower)
                {
                    spot.RegistrationId_Follower = registrationId;
                }
                else
                {
                    // also fallback for missing role
                    spot.RegistrationId = registrationId;
                }
            }
            else
            {
                // single registration
                var waitingListForSingleLeaders = waitingList.Any(spt => string.IsNullOrEmpty(spt.PartnerEmail)
                                                                      && spt.RegistrationId.HasValue);
                var waitingListForSingleFollowers = waitingList.Any(spt => string.IsNullOrEmpty(spt.PartnerEmail)
                                                                        && spt.RegistrationId_Follower.HasValue);

                var waitingListForOwnRole = (ourRole == Role.Leader && waitingListForSingleLeaders)
                                         || (ourRole == Role.Follower && waitingListForSingleFollowers)
                                         || ourRole == null;
                var matchingSingleSeat = FindMatchingSingleSeat(spots, ourRole);
                var seatAvailable = !waitingListForOwnRole
                                 && (imbalanceManager.CanAddNewDoubleSpotForSingleRegistration(
                                         registrable.MaximumDoubleSeats.Value,
                                         registrable.MaximumAllowedImbalance ?? 0,
                                         spots,
                                         ourRole)
                                  || matchingSingleSeat != null);
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }

                if ((ourRole == Role.Leader && waitingListForSingleFollowers)
                 || (ourRole == Role.Follower && waitingListForSingleLeaders))
                {
                    // ToDo: check waiting list
                    //registrableId_CheckWaitingList = registrable.Id;
                }

                if (!waitingListForOwnRole && matchingSingleSeat != null)
                {
                    ComplementExistingSeat(registrationId, ourRole, matchingSingleSeat);
                    eventBus.Publish(new SpotAdded
                                     {
                                         Id = Guid.NewGuid(),
                                         EventId = eventId,
                                         RegistrableId = registrable.Id,
                                         Registrable = registrable.DisplayName,
                                         RegistrationId = registrationId,
                                         IsInitialProcessing = initialProcessing,
                                         IsWaitingList = matchingSingleSeat.IsWaitingList
                                     });
                    return matchingSingleSeat;
                }

                spot = new Seat
                       {
                           FirstPartnerJoined = dateTimeProvider.Now,
                           RegistrableId = registrable.Id,
                           IsWaitingList = !seatAvailable
                                           && ourRole != null
                };

                if (ourRole == Role.Follower)
                {
                    spot.RegistrationId_Follower = registrationId;
                }
                else
                {
                    // also fallback for missing role
                    spot.RegistrationId = registrationId;
                }
            }
        }
        else
        {
            // no limit
            spot = new Seat
                   {
                       RegistrationId = registrationId,
                       RegistrableId = registrable.Id,
                       FirstPartnerJoined = dateTimeProvider.Now
                   };
        }

        spot.Id = Guid.NewGuid();
        _spots.InsertObjectTree(spot);
        eventBus.Publish(new SpotAdded
                         {
                             Id = Guid.NewGuid(),
                             EventId = eventId,
                             RegistrableId = registrable.Id,
                             Registrable = registrable.DisplayName,
                             RegistrationId = registrationId,
                             IsInitialProcessing = initialProcessing,
                             IsWaitingList = spot.IsWaitingList
                         });

        return spot;
    }

    public async Task<Seat?> ReserveSingleSpot(Guid? eventId,
                                               Guid registrableId,
                                               Guid registrationId,
                                               bool initialProcessing)
    {
        var registrable = await registrables.Where(rbl => rbl.Id == registrableId)
                                            .Include(rbl => rbl.Spots)
                                            .FirstAsync();
        var seats = registrable.Spots!
                               .Where(st => !st.IsCancelled)
                               .ToList();
        if (registrable.MaximumDoubleSeats != null)
        {
            throw new InvalidOperationException("Unexpected: Attempt to reserve single spot as partner spot");
        }

        var seat = new Seat
                   {
                       Id = Guid.NewGuid(),
                       RegistrationId = registrationId,
                       RegistrableId = registrable.Id,
                       FirstPartnerJoined = dateTimeProvider.Now
                   };
        if (registrable.MaximumSingleSeats != null)
        {
            var waitingList = seats.Any(st => st.IsWaitingList);
            var seatAvailable = !waitingList && seats.Count < registrable.MaximumSingleSeats.Value;
            if (!seatAvailable && !registrable.HasWaitingList)
                // no spot available, no waiting list
            {
                return null;
            }

            seat.IsWaitingList = !seatAvailable;
        }

        _spots.InsertObjectTree(seat);
        eventBus.Publish(new SpotAdded
                         {
                             Id = Guid.NewGuid(),
                             RegistrableId = registrable.Id,
                             Registrable = registrable.DisplayName,
                             RegistrationId = registrationId,
                             IsInitialProcessing = initialProcessing,
                             IsWaitingList = seat.IsWaitingList
                         });

        return seat;
    }

    public void RemoveSpot(Seat spot, Guid registrationId, RemoveSpotReason reason)
    {
        if (spot.RegistrationId == registrationId)
        {
            if (spot.RegistrationId_Follower != null)
            {
                // double spot, leave the partner in
                spot.RegistrationId = null;
                spot.PartnerEmail = null;
                spot.IsPartnerSpot = false;
            }
            else
            {
                // single spot, cancel the place
                spot.IsCancelled = true;
            }
        }
        else if (spot.RegistrationId_Follower == registrationId)
        {
            if (spot.RegistrationId != null)
            {
                // double spot, leave the partner in
                spot.RegistrationId_Follower = null;
                spot.PartnerEmail = null;
                spot.IsPartnerSpot = false;
            }
            else
            {
                // single spot, cancel the place
                spot.IsCancelled = true;
            }
        }

        var registration = registrations.First(reg => reg.Id == registrationId);
        var registrable = registrables.First(rbl => rbl.Id == spot.RegistrableId);
        eventBus.Publish(new SpotRemoved
                         {
                             Id = Guid.NewGuid(),
                             RegistrableId = spot.RegistrableId,
                             RegistrationId = registrationId,
                             Reason = reason,
                             SpotWasOnWaitingList = spot.IsWaitingList,
                             Participant = $"{registration.RespondentFirstName} {registration.RespondentLastName}",
                             Registrable = registrable.DisplayName
                         });
    }

    private static void ComplementExistingSeat(Guid registrationId, Role? ownRole, Seat existingSeat)
    {
        if (existingSeat.RegistrationId == null
         && ownRole != Role.Follower)
        {
            existingSeat.RegistrationId = registrationId;
        }
        else if (existingSeat.RegistrationId_Follower == null
              && ownRole != Role.Leader)
        {
            existingSeat.RegistrationId_Follower = registrationId;
        }
        else
        {
            throw new Exception(
                $"Unexpected situation: Own Role {ownRole}, partner seat registrationId {existingSeat.RegistrationId}/registrationId_Follower {existingSeat.RegistrationId_Follower}");
        }
    }

    private static Seat? FindMatchingSingleSeat(IEnumerable<Seat> spots, Role? ownRole)
    {
        if (ownRole == null)
        {
            return null;
        }

        return spots?.FirstOrDefault(spt => string.IsNullOrEmpty(spt.PartnerEmail)
                                         && !spt.IsWaitingList
                                         && ((ownRole == Role.Leader && !spt.RegistrationId.HasValue)
                                          || (ownRole == Role.Follower && !spt.RegistrationId_Follower.HasValue)));
    }

    private async Task<Seat?> FindPartnerSpot(Guid eventId,
                                              RegistrationIdentification ownIdentification,
                                              string? partner,
                                              Guid? registrationId_Partner,
                                              Role? ownRole,
                                              ICollection<Seat> existingSpots)
    {
        var potentialPartnerSpots = existingSpots.Where(spt => spt.IsPartnerSpot
                                                               // open partner spot
                                                            && (spt.RegistrationId == null || spt.RegistrationId_Follower == null))
                                                 .ToList();
        var partnerSpots = potentialPartnerSpots.Where(spt => spt.PartnerEmail == ownIdentification.Email)
                                                .ToList();
        if (partnerSpots.Count == 0)
        {
            partnerSpots = potentialPartnerSpots.Where(spt => $" {spt.PartnerEmail} ".Contains($" {ownIdentification.FirstName} ")
                                                           && $" {spt.PartnerEmail} ".Contains($" {ownIdentification.LastName} "))
                                                .ToList();
            if (partnerSpots.Count == 0)
            {
                return null;
            }
        }

        foreach (var partnerSpot in partnerSpots)
        {
            if (ownRole == Role.Leader && partnerSpot.RegistrationId != null
             || ownRole == Role.Follower && partnerSpot.RegistrationId_Follower != null)
            {
                // role mismatch
                continue;
            }

            var otherRegistrationId = partnerSpot.RegistrationId
                                   ?? partnerSpot.RegistrationId_Follower;
            if (registrationId_Partner != null
             && registrationId_Partner != otherRegistrationId)
            {
                // partner mismatch
                continue;
            }

            var otherRegistration = await registrations.FirstOrDefaultAsync(reg => reg.RegistrationForm!.EventId == eventId
                                                                                && reg.Id == otherRegistrationId);
            if (otherRegistration.IsAlreadyMatchedToOtherThan(ownIdentification.Id))
            {
                // other registration is already has a different partner
                continue;
            }

            if (otherRegistration.MatchesPartnerText(partner))
            {
                return partnerSpot;
            }
        }

        return null;
    }
}