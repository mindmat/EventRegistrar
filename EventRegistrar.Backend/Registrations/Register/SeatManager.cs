using System;
using System.Collections.Generic;
using System.Linq;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Spots;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class SeatManager
    {
        private readonly EventBus _eventBus;
        private readonly ImbalanceManager _imbalanceManager;
        private readonly ILogger _logger;
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Seat> _seats;

        public SeatManager(IRepository<Seat> seats,
                           ImbalanceManager imbalanceManager,
                           ILogger logger,
                           IQueryable<Registration> registrations,
                           EventBus eventBus)
        {
            _seats = seats;
            _imbalanceManager = imbalanceManager;
            _logger = logger;
            _registrations = registrations;
            _eventBus = eventBus;
        }

        public Seat ReservePartnerSpot(Guid? eventId,
                                       Registrable registrable,
                                       Guid registrationId_Leader,
                                       Guid registrationId_Follower)
        {
            var seats = registrable.Seats.Where(st => !st.IsCancelled).ToList();
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
                FirstPartnerJoined = DateTime.UtcNow
            };
            if (registrable.MaximumDoubleSeats.HasValue)
            {
                var waitingListForPartnerRegistrations = seats.Any(st => st.IsWaitingList && st.IsPartnerSpot);
                var seatAvailable = !waitingListForPartnerRegistrations && seats.Count < registrable.MaximumDoubleSeats.Value;
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    // no spot available, no waiting list
                    return null;
                }

                seat.IsWaitingList = !seatAvailable;
            }

            _seats.InsertOrUpdateEntity(seat);
            _eventBus.Publish(new SpotAdded { RegistrableId = registrable.Id, RegistrationId = registrationId_Leader });
            _eventBus.Publish(new SpotAdded { RegistrableId = registrable.Id, RegistrationId = registrationId_Follower });
            return seat;
        }

        public Seat ReserveSinglePartOfPartnerSpot(Guid? eventId,
                                                   Registrable registrable,
                                                   Guid registrationId,
                                                   RegistrationIdentification ownIdentification,
                                                   string partner,
                                                   Role? role)
        {
            Seat seat;
            var seats = registrable.Seats.Where(st => !st.IsCancelled).ToList();
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var waitingList = seats.Any(st => st.IsWaitingList);
                var seatAvailable = !waitingList && seats.Count < registrable.MaximumSingleSeats.Value;
                _logger.LogInformation($"Registrable {registrable.Name}, Seat count {seats.Count}, MaximumSingleSeats {registrable.MaximumSingleSeats}, seat available {seatAvailable}");
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }
                seat = new Seat
                {
                    FirstPartnerJoined = DateTime.UtcNow,
                    RegistrationId = registrationId,
                    RegistrableId = registrable.Id,
                    IsWaitingList = !seatAvailable
                };
            }
            else if (registrable.MaximumDoubleSeats.HasValue)
            {
                if (!role.HasValue)
                {
                    throw new Exception("No role found");
                }
                var isPartnerRegistration = !string.IsNullOrEmpty(partner);
                var ownRole = role.Value;
                var waitingList = seats.Where(st => st.IsWaitingList).ToList();
                if (isPartnerRegistration)
                {
                    // complement existing partner seat
                    var existingPartnerSeat = FindPartnerSeat(eventId, ownIdentification, partner, ownRole, seats);

                    if (existingPartnerSeat != null)
                    {
                        ComplementExistingSeat(registrationId, ownRole, existingPartnerSeat);
                        _eventBus.Publish(new SpotAdded { RegistrableId = registrable.Id, RegistrationId = registrationId });
                        return existingPartnerSeat;
                    }

                    // create new partner seat
                    var waitingListForPartnerRegistrations = waitingList.Any(st => !string.IsNullOrEmpty(st.PartnerEmail));
                    var seatAvailable = !waitingListForPartnerRegistrations && seats.Count < registrable.MaximumDoubleSeats.Value;
                    if (!seatAvailable && !registrable.HasWaitingList)
                    {
                        return null;
                    }
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
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
                    var waitingListForSingleLeaders = waitingList.Any(st => string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId.HasValue);
                    var waitingListForSingleFollowers = waitingList.Any(st => string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId_Follower.HasValue);

                    var waitingListForOwnRole = ownRole == Role.Leader && waitingListForSingleLeaders ||
                                                ownRole == Role.Follower && waitingListForSingleFollowers;
                    var matchingSingleSeat = FindMatchingSingleSeat(seats, ownRole);
                    var seatAvailable = !waitingListForOwnRole && (_imbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrable, ownRole) || matchingSingleSeat != null);
                    if (!seatAvailable && !registrable.HasWaitingList)
                    {
                        return null;
                    }
                    if (ownRole == Role.Leader && waitingListForSingleFollowers ||
                        ownRole == Role.Follower && waitingListForSingleLeaders)
                    {
                        // ToDo: check waiting list
                        //registrableId_CheckWaitingList = registrable.Id;
                    }
                    if (!waitingListForOwnRole && matchingSingleSeat != null)
                    {
                        ComplementExistingSeat(registrationId, ownRole, matchingSingleSeat);
                        _eventBus.Publish(new SpotAdded { RegistrableId = registrable.Id, RegistrationId = registrationId });
                        return matchingSingleSeat;
                    }
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
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
                    FirstPartnerJoined = DateTime.UtcNow
                };
            }

            seat.Id = Guid.NewGuid();
            _seats.InsertOrUpdateEntity(seat);
            _eventBus.Publish(new SpotAdded { RegistrableId = registrable.Id, RegistrationId = registrationId });

            return seat;
        }

        public Seat ReserveSingleSpot(Guid? eventId,
                                      Registrable registrable,
                                      Guid registrationId)
        {
            var seats = registrable.Seats.Where(st => !st.IsCancelled).ToList();
            if (registrable.MaximumDoubleSeats.HasValue)
            {
                throw new InvalidOperationException("Unexpected: Attempt to reserve single spot as partner spot");
            }

            var seat = new Seat
            {
                Id = Guid.NewGuid(),
                RegistrationId = registrationId,
                RegistrableId = registrable.Id,
                FirstPartnerJoined = DateTime.UtcNow,
            };
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var waitingList = seats.Any(st => st.IsWaitingList);
                var seatAvailable = !waitingList && seats.Count < registrable.MaximumSingleSeats.Value;
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    // no spot available, no waiting list
                    return null;
                }

                seat.IsWaitingList = !seatAvailable;
            }

            _seats.InsertOrUpdateEntity(seat);
            _eventBus.Publish(new SpotAdded { RegistrableId = registrable.Id, RegistrationId = registrationId });

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
                throw new Exception($"Unexpected situation: Own Role {ownRole}, partner seat registrationId {existingSeat.RegistrationId}/registrationId_Follower {existingSeat.RegistrationId_Follower}");
            }
        }

        private static Seat FindMatchingSingleSeat(IEnumerable<Seat> seats, Role ownRole)
        {
            return seats?.FirstOrDefault(seat => string.IsNullOrEmpty(seat.PartnerEmail) &&
                                                 !seat.IsWaitingList &&
                                                 (ownRole == Role.Leader && !seat.RegistrationId.HasValue ||
                                                  ownRole == Role.Follower && !seat.RegistrationId_Follower.HasValue));
        }

        private Seat FindPartnerSeat(Guid? eventId,
                                     RegistrationIdentification ownIdentification,
                                     string partner,
                                     Role ownRole,
                                     ICollection<Seat> existingSeats)
        {
            var potentialPartnerSeats = existingSeats.Where(seat => seat.IsPartnerSpot
                                                                 // own part still available
                                                                 && (ownRole == Role.Leader && seat.RegistrationId == null
                                                                  || ownRole == Role.Follower && seat.RegistrationId_Follower == null))
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
            var partnerRegistrationIds = partnerSeats.Select(seat => otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower)
                                                     .ToList();
            var registrationsThatReferenceOwnRegistration = _registrations.Where(reg => (!eventId.HasValue || reg.RegistrationForm.EventId == eventId.Value)
                                                                                     && partnerRegistrationIds.Contains(reg.Id))
                                                                          .ToList();
            _logger.LogInformation($"Partner registrations with this partner mail: {string.Join(", ", registrationsThatReferenceOwnRegistration.Select(reg => $"{reg.Id} ({reg.RespondentFirstName} {reg.RespondentLastName} - {reg.RespondentEmail})"))}");
            var partnerRegistrationId = registrationsThatReferenceOwnRegistration.FirstOrDefault(reg => string.Equals(reg.RespondentEmail, partner, StringComparison.InvariantCultureIgnoreCase))?.Id;
            if (partnerRegistrationId == null)
            {
                partnerRegistrationId = registrationsThatReferenceOwnRegistration.FirstOrDefault(reg => $" {partner} ".Contains($" {reg.RespondentFirstName} ", StringComparison.InvariantCultureIgnoreCase)
                                                                                                     && $" {partner} ".Contains($" {reg.RespondentLastName} ", StringComparison.InvariantCultureIgnoreCase))?.Id;
            }

            return partnerSeats.FirstOrDefault(seat => partnerRegistrationId == (otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower));
        }
    }
}