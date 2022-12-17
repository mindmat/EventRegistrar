using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Register;

public class SpotManager
{
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ImbalanceManager _imbalanceManager;
    private readonly ILogger _logger;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _registrables;
    private readonly IRepository<Seat> _seats;

    public SpotManager(IRepository<Seat> seats,
                       ImbalanceManager imbalanceManager,
                       ILogger logger,
                       IQueryable<Registration> registrations,
                       IQueryable<Registrable> registrables,
                       IEventBus eventBus,
                       IDateTimeProvider dateTimeProvider)
    {
        _seats = seats;
        _imbalanceManager = imbalanceManager;
        _logger = logger;
        _registrations = registrations;
        _registrables = registrables;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

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
                       FirstPartnerJoined = _dateTimeProvider.Now
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

        _seats.InsertObjectTree(seat);
        _eventBus.Publish(new SpotAdded
                          {
                              Id = Guid.NewGuid(),
                              EventId = eventId,
                              RegistrableId = registrable.Id,
                              Registrable = registrable.DisplayName,
                              RegistrationId = registrationId_Leader,
                              IsInitialProcessing = initialProcessing,
                              IsWaitingList = seat.IsWaitingList
                          });
        _eventBus.Publish(new SpotAdded
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
                                                            RegistrationIdentification ownIdentification,
                                                            string? partner,
                                                            Guid? registrationId_Partner,
                                                            Role? role,
                                                            bool initialProcessing)
    {
        Seat seat;
        var registrable = await _registrables.Where(rbl => rbl.Id == registrableId)
                                             .Include(rbl => rbl.Spots)
                                             .FirstAsync();
        var spots = registrable.Spots!.Where(st => !st.IsCancelled)
                               .ToList();
        if (registrable.MaximumSingleSeats != null)
        {
            var waitingList = spots.Any(st => st.IsWaitingList);
            var spotAvailable = !waitingList && spots.Count < registrable.MaximumSingleSeats.Value;
            _logger.LogInformation($"Registrable {registrable.DisplayName}, Seat count {spots.Count}, MaximumSingleSeats {registrable.MaximumSingleSeats}, seat available {spotAvailable}");
            if (!spotAvailable && !registrable.HasWaitingList)
            {
                return null;
            }

            seat = new Seat
                   {
                       FirstPartnerJoined = _dateTimeProvider.Now,
                       RegistrationId = registrationId,
                       RegistrableId = registrable.Id,
                       IsWaitingList = !spotAvailable
                   };
        }
        else if (registrable.MaximumDoubleSeats != null)
        {
            if (!role.HasValue)
            {
                throw new Exception("No role found");
            }

            var isPartnerRegistration = !string.IsNullOrEmpty(partner) || registrationId_Partner != null;
            var ownRole = role.Value;
            var waitingList = spots.Where(st => st.IsWaitingList).ToList();
            if (isPartnerRegistration)
            {
                // complement existing partner seat
                var existingPartnerSeat = await FindPartnerSeat(eventId, ownIdentification, partner, registrationId_Partner, ownRole,
                                                                spots);

                if (existingPartnerSeat != null)
                {
                    ComplementExistingSeat(registrationId, ownRole, existingPartnerSeat);
                    _eventBus.Publish(new SpotAdded
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
                var seatAvailable = !waitingListForPartnerRegistrations && spots.Count < registrable.MaximumDoubleSeats.Value;
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }

                seat = new Seat
                       {
                           FirstPartnerJoined = _dateTimeProvider.Now,
                           PartnerEmail = partner?.ToLowerInvariant(),
                           RegistrationId = ownRole == Role.Leader ? registrationId : (Guid?)null,
                           RegistrationId_Follower = ownRole == Role.Follower ? registrationId : (Guid?)null,
                           RegistrableId = registrable.Id,
                           IsWaitingList = !seatAvailable,
                           IsPartnerSpot = true
                       };
            }
            else
            {
                // single registration
                var waitingListForSingleLeaders = waitingList.Any(st =>
                                                                      string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId.HasValue);
                var waitingListForSingleFollowers = waitingList.Any(st =>
                                                                        string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId_Follower.HasValue);

                var waitingListForOwnRole = (ownRole == Role.Leader && waitingListForSingleLeaders) || (ownRole == Role.Follower && waitingListForSingleFollowers);
                var matchingSingleSeat = FindMatchingSingleSeat(spots, ownRole);
                var seatAvailable = !waitingListForOwnRole
                                 && (_imbalanceManager.CanAddNewDoubleSeatForSingleRegistration(
                                         registrable.MaximumDoubleSeats.Value,
                                         registrable.MaximumAllowedImbalance ?? 0,
                                         spots,
                                         ownRole)
                                  || matchingSingleSeat != null);
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }

                if ((ownRole == Role.Leader && waitingListForSingleFollowers) || (ownRole == Role.Follower && waitingListForSingleLeaders))
                {
                    // ToDo: check waiting list
                    //registrableId_CheckWaitingList = registrable.Id;
                }

                if (!waitingListForOwnRole && matchingSingleSeat != null)
                {
                    ComplementExistingSeat(registrationId, ownRole, matchingSingleSeat);
                    _eventBus.Publish(new SpotAdded
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

                seat = new Seat
                       {
                           FirstPartnerJoined = _dateTimeProvider.Now,
                           RegistrationId = ownRole == Role.Leader ? registrationId : (Guid?)null,
                           RegistrationId_Follower = ownRole == Role.Follower ? registrationId : (Guid?)null,
                           RegistrableId = registrable.Id,
                           IsWaitingList = !seatAvailable
                       };
            }
        }
        else
        {
            // no limit
            seat = new Seat
                   {
                       RegistrationId = registrationId,
                       RegistrableId = registrable.Id,
                       FirstPartnerJoined = _dateTimeProvider.Now
                   };
        }

        seat.Id = Guid.NewGuid();
        await _seats.InsertOrUpdateEntity(seat);
        _eventBus.Publish(new SpotAdded
                          {
                              Id = Guid.NewGuid(),
                              EventId = eventId,
                              RegistrableId = registrable.Id,
                              Registrable = registrable.DisplayName,
                              RegistrationId = registrationId,
                              IsInitialProcessing = initialProcessing,
                              IsWaitingList = seat.IsWaitingList
                          });

        return seat;
    }

    public async Task<Seat?> ReserveSingleSpot(Guid? eventId,
                                               Guid registrableId,
                                               Guid registrationId,
                                               bool initialProcessing)
    {
        var registrable = await _registrables.Where(rbl => rbl.Id == registrableId)
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
                       FirstPartnerJoined = _dateTimeProvider.Now
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

        _seats.InsertObjectTree(seat);
        _eventBus.Publish(new SpotAdded
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

    private static void ComplementExistingSeat(Guid registrationId, Role ownRole, Seat existingSeat)
    {
        if (ownRole == Role.Leader && !existingSeat.RegistrationId.HasValue)
        {
            existingSeat.RegistrationId = registrationId;
        }
        else if (ownRole == Role.Follower && !existingSeat.RegistrationId_Follower.HasValue)
        {
            existingSeat.RegistrationId_Follower = registrationId;
        }
        else
        {
            throw new Exception(
                $"Unexpected situation: Own Role {ownRole}, partner seat registrationId {existingSeat.RegistrationId}/registrationId_Follower {existingSeat.RegistrationId_Follower}");
        }
    }

    private static Seat FindMatchingSingleSeat(IEnumerable<Seat> seats, Role ownRole)
    {
        return seats?.FirstOrDefault(seat => string.IsNullOrEmpty(seat.PartnerEmail)
                                          && !seat.IsWaitingList
                                          && ((ownRole == Role.Leader && !seat.RegistrationId.HasValue) || (ownRole == Role.Follower && !seat.RegistrationId_Follower.HasValue)));
    }

    private async Task<Seat?> FindPartnerSeat(Guid eventId,
                                              RegistrationIdentification ownIdentification,
                                              string? partner,
                                              Guid? registrationId_Partner,
                                              Role ownRole,
                                              ICollection<Seat> existingSeats)
    {
        var potentialPartnerSeats = existingSeats.Where(seat => seat.IsPartnerSpot
                                                                // own part still available
                                                             && ((ownRole == Role.Leader && seat.RegistrationId == null)
                                                              || (ownRole == Role.Follower && seat.RegistrationId_Follower == null)))
                                                 .ToList();
        var partnerSeats = potentialPartnerSeats.Where(seat => seat.PartnerEmail == ownIdentification.Email)
                                                .ToList();
        if (!partnerSeats.Any())
        {
            partnerSeats = potentialPartnerSeats.Where(seat => $" {seat.PartnerEmail} ".Contains($" {ownIdentification.FirstName} ")
                                                            && $" {seat.PartnerEmail} ".Contains($" {ownIdentification.LastName} "))
                                                .ToList();
            if (!partnerSeats.Any())
            {
                return null;
            }
        }

        _logger.LogInformation($"partner seats ids: {string.Join(", ", partnerSeats.Select(seat => seat.Id))}");

        var otherRole = ownRole == Role.Leader ? Role.Follower : Role.Leader;
        var partnerRegistrationIds = partnerSeats
                                     .Select(seat => otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower)
                                     .ToList();
        var registrationsThatReferenceOwnRegistration = await _registrations.Where(reg => reg.RegistrationForm!.EventId == eventId
                                                                                       && (reg.RegistrationId_Partner == null || reg.RegistrationId_Partner == ownIdentification.Id)
                                                                                       && partnerRegistrationIds.Contains(reg.Id))
                                                                            .ToListAsync();
        _logger.LogInformation(
            $"Partner registrations with this partner mail: {string.Join(", ", registrationsThatReferenceOwnRegistration.Select(reg => $"{reg.Id} ({reg.RespondentFirstName} {reg.RespondentLastName} - {reg.RespondentEmail})"))}");

        registrationId_Partner ??= registrationsThatReferenceOwnRegistration.FirstOrDefault(reg => string.Equals(reg.RespondentEmail,
                                                                                                                 partner,
                                                                                                                 StringComparison.InvariantCultureIgnoreCase))
                                                                            ?.Id
                                ?? registrationsThatReferenceOwnRegistration.FirstOrDefault(reg => $" {partner} ".Contains($" {reg.RespondentFirstName} ",
                                                                                                                           StringComparison.InvariantCultureIgnoreCase)
                                                                                                && $" {partner} ".Contains($" {reg.RespondentLastName} ",
                                                                                                                           StringComparison.InvariantCultureIgnoreCase))
                                                                            ?.Id;

        return partnerSeats.FirstOrDefault(seat => registrationId_Partner == (otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower));
    }
}