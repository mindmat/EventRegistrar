using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Properties;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Seats;
using EventRegistrator.Functions.WaitingList;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.Registrables
{
    public static class ProcessNewRegistration
    {
        public const string ReceivedRegistrationsQueueName = "ReceivedRegistrations";

        [FunctionName("ProcessNewRegistration")]
        public static async Task Run(
            [ServiceBusTrigger(ReceivedRegistrationsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]
            RegistrationRegistered @event, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {@event}");
            log.Info($"id {@event.RegistrationId}");

            var ownSeats = new List<Seat>();
            using (var context = new EventRegistratorDbContext())
            {
                var registration = await context.Registrations.FirstOrDefaultAsync(reg => reg.Id == @event.RegistrationId);
                if (registration == null)
                {
                    throw new Exception($"Invalid RegistrationId received {@event.RegistrationId}");
                }
                var responses = await context.Responses.Where(rsp => rsp.RegistrationId == @event.RegistrationId).ToListAsync();
                var questionOptionIds = new HashSet<Guid>(responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value));
                var registrables = await context.QuestionOptionToRegistrableMappings
                                                .Where(map => questionOptionIds.Contains(map.QuestionOptionId))
                                                .Include(map => map.Registrable)
                                                .Include(map => map.Registrable.Seats)
                                                .ToListAsync();
                var registrableIds_CheckWaitingList = new List<Guid>();
                foreach (var response in responses.Where(rsp => rsp.QuestionOptionId.HasValue))
                {
                    foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
                    {
                        var partnerEmail = registrable.QuestionId_PartnerEmail.HasValue
                            ? responses.FirstOrDefault(rsp => rsp.QuestionId == registrable.QuestionId_PartnerEmail.Value)?.ResponseString
                            : null;
                        var isLeader = registrable.QuestionOptionId_Leader.HasValue &&
                                       responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Leader.Value);
                        var isFollower = registrable.QuestionOptionId_Follower.HasValue &&
                                         responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Follower.Value);
                        var role = isLeader ? Role.Leader : (isFollower ? Role.Follower : (Role?)null);
                        var seat = ReserveSeat(context, @event.EventId, registrable.Registrable, response, registration.RespondentEmail, partnerEmail, role, log, out Guid? registrableId_CheckWaitingList);
                        if (registrableId_CheckWaitingList != null)
                        {
                            registrableIds_CheckWaitingList.Add(registrableId_CheckWaitingList.Value);
                        }
                        if (seat == null)
                        {
                            registration.SoldOutMessage = (registration.SoldOutMessage == null ? string.Empty : registration.SoldOutMessage + Environment.NewLine) +
                                                          string.Format(Resources.RegistrableSoldOut, registrable.Registrable.Name);
                        }
                        else
                        {
                            ownSeats.Add(seat);
                        }
                    }
                }
                var eventRegistrables = await context.Registrables.Where(rbl => rbl.EventId == @event.EventId).ToListAsync();
                var registrableIds = new HashSet<Guid>(eventRegistrables.Select(rbl => rbl.Id));
                var reductions = await context.Reductions.Where(red => registrableIds.Contains(red.RegistrableId)).ToListAsync();
                registration.Price = CalculatePrice(ownSeats, eventRegistrables, reductions, questionOptionIds);
                registration.IsWaitingList = ownSeats.Any(seat => seat.IsWaitingList);

                await context.SaveChangesAsync();

                await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registration.Id }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
                if (registrableIds_CheckWaitingList.Any() && @event.EventId.HasValue)
                {
                    foreach (var registrableId in registrableIds_CheckWaitingList)
                    {
                        await ServiceBusClient.SendEvent(new TryPromoteFromWaitingListCommand { RegistrableId = registrableId }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
                    }
                }
            }
        }

        private static decimal CalculatePrice(IReadOnlyCollection<Seat> seats, 
                                              IReadOnlyCollection<Registrable> registrables,
                                              IReadOnlyCollection<Reduction> reductions,
                                              ICollection<Guid> questionOptionIds)
        {
            var bookedRegistrableIds = new HashSet<Guid>(seats.Select(seat => seat.RegistrableId));
            var price = 0m;
            foreach (var seat in seats)
            {
                var registrable = registrables.FirstOrDefault(reg => reg.Id == seat.RegistrableId);
                if (registrable == null)
                {
                    continue;
                }
                price += registrable.Price ?? 0m;
                var potentialReductions = reductions.Where(red => red.RegistrableId == seat.RegistrableId).ToList();
                var applicableReductions = potentialReductions.Where(red => red.QuestionOptionId_ActivatesReduction.HasValue &&
                                                                            questionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value) &&
                                                                            !red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue)
                                                              .ToList();

                applicableReductions.AddRange(potentialReductions.Where(red => red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue && bookedRegistrableIds.Contains(red.RegistrableId1_ReductionActivatedIfCombinedWith.Value) &&
                                                                               (!red.RegistrableId2_ReductionActivatedIfCombinedWith.HasValue || bookedRegistrableIds.Contains(red.RegistrableId2_ReductionActivatedIfCombinedWith.Value)) &&
                                                                               (!red.QuestionOptionId_ActivatesReduction.HasValue || questionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value))));

                price -= applicableReductions.Sum(red => red.Amount);
            }

            return price;
        }

        private static Seat ReserveSeat(EventRegistratorDbContext context,
                                        Guid? eventId,
                                        Registrable registrable,
                                        Response response,
                                        string ownEmail,
                                        string partnerEmail,
                                        Role? role,
                                        TraceWriter log,
                                        out Guid? registrableId_CheckWaitingList)
        {
            Seat seat;
            registrableId_CheckWaitingList = null;
            var seats = registrable.Seats.Where(st => !st.IsCancelled).ToList();
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var waitingList = seats.Any(st => st.IsWaitingList);
                var seatAvailable = !waitingList && seats.Count < registrable.MaximumSingleSeats.Value;
                log.Info($"Registrable {registrable.Name}, Seat count {seats.Count}, MaximumSingleSeats {registrable.MaximumSingleSeats}, seat available {seatAvailable}");
                if (!seatAvailable && !registrable.HasWaitingList)
                {
                    return null;
                }
                seat = new Seat
                {
                    FirstPartnerJoined = DateTime.UtcNow,
                    RegistrationId = response.RegistrationId,
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
                var isPartnerRegistration = !string.IsNullOrEmpty(partnerEmail);
                var ownRole = role.Value;
                var waitingList = seats.Where(st => st.IsWaitingList).ToList();
                if (isPartnerRegistration)
                {
                    // complement existing partner seat
                    var existingPartnerSeat = FindPartnerSeat(eventId, ownEmail, partnerEmail, ownRole, seats, context.Registrations, log);

                    if (existingPartnerSeat != null)
                    {
                        ComplementExistingSeat(response.RegistrationId, ownRole, existingPartnerSeat);
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
                        PartnerEmail = partnerEmail,
                        RegistrationId = ownRole == Role.Leader ? response.RegistrationId : (Guid?)null,
                        RegistrationId_Follower = ownRole == Role.Follower ? response.RegistrationId : (Guid?)null,
                        RegistrableId = registrable.Id,
                        IsWaitingList = !seatAvailable
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
                    var seatAvailable = !waitingListForOwnRole && (ImbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrable, ownRole) || matchingSingleSeat != null);
                    if (!seatAvailable && !registrable.HasWaitingList)
                    {
                        return null;
                    }
                    if (ownRole == Role.Leader && waitingListForSingleFollowers ||
                        ownRole == Role.Follower && waitingListForSingleLeaders)
                    {
                        registrableId_CheckWaitingList = registrable.Id;
                    }
                    if (!waitingListForOwnRole && matchingSingleSeat != null)
                    {
                        ComplementExistingSeat(response.RegistrationId, ownRole, matchingSingleSeat);
                        return matchingSingleSeat;
                    }
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
                        RegistrationId = ownRole == Role.Leader ? response.RegistrationId : (Guid?)null,
                        RegistrationId_Follower = ownRole == Role.Follower ? response.RegistrationId : (Guid?)null,
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
                    RegistrationId = response.RegistrationId,
                    RegistrableId = registrable.Id,
                    FirstPartnerJoined = DateTime.UtcNow
                };
            }

            seat.Id = Guid.NewGuid();
            context.Seats.Add(seat);

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

        private static Seat FindPartnerSeat(Guid? eventId, string ownEmail, string partnerEmail, Role ownRole, ICollection<Seat> existingSeats, IQueryable<Registration> registrations, TraceWriter log)
        {
            var partnerSeats = existingSeats.Where(seat => seat.PartnerEmail == ownEmail).ToList();
            if (!partnerSeats.Any())
            {
                return null;
            }
            var otherRole = ownRole == Role.Leader ? Role.Follower : Role.Leader;
            var partnerRegistrationIds = partnerSeats.Select(seat => otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower).ToList();
            var partnerRegistrationThatReferenceThisEmail = registrations.Where(reg => (!eventId.HasValue || reg.RegistrationForm.EventId == eventId.Value) &&
                                                                                       partnerRegistrationIds.Contains(reg.Id))
                                                                         .ToList();
            log.Info($"Partner registrations with this partner mail: {string.Join(", ", partnerRegistrationThatReferenceThisEmail.Select(reg => reg.Id))}");
            var partnerRegistrationId = partnerRegistrationThatReferenceThisEmail.FirstOrDefault(reg => reg.RespondentEmail == partnerEmail)?.Id;
            return partnerSeats.FirstOrDefault(seat => partnerRegistrationId == (otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower));
        }
    }
}