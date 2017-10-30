using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Seats;
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

                foreach (var response in responses.Where(rsp => rsp.QuestionOptionId.HasValue))
                {
                    foreach (var registrable in registrables.Where(rbl =>
                        rbl.QuestionOptionId == response.QuestionOptionId))
                    {
                        var partnerEmail = registrable.QuestionId_PartnerEmail.HasValue
                            ? responses.FirstOrDefault(
                                rsp => rsp.QuestionId == registrable.QuestionId_PartnerEmail.Value)?.ResponseString
                            : null;
                        var isLeader = registrable.QuestionOptionId_Leader.HasValue &&
                                       responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Leader.Value);
                        var isFollower = registrable.QuestionOptionId_Follower.HasValue &&
                                         responses.Any(rsp => rsp.QuestionOptionId == registrable.QuestionOptionId_Follower.Value);
                        var role = isLeader ? Role.Leader : (isFollower ? Role.Follower : (Role?)null);
                        var seat = await ReserveSeat(context, @event.EventId, registrable.Registrable, response, @event.Registration.RespondentEmail, partnerEmail, role, log);
                        ownSeats.Add(seat);
                    }
                }
                var eventRegistrables = await context.Registrables.Where(rbl => rbl.EventId == @event.EventId).ToListAsync();
                var registrableIds = new HashSet<Guid>(eventRegistrables.Select(rbl => rbl.Id));
                var reductions = await context.Reductions.Where(red => registrableIds.Contains(red.RegistrableId)).ToListAsync();
                registration.Price = CalculatePrice(ownSeats, eventRegistrables, reductions, questionOptionIds);

                await context.SaveChangesAsync();

                await SendStatusMail(eventRegistrables, ownSeats, @event.RegistrationId);
            }
        }

        private static decimal CalculatePrice(List<Seat> seats, List<Registrable> registrables, List<Reduction> reductions, HashSet<Guid> questionOptionIds)
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
                var applicableReductions = potentialReductions.Where(red => red.QuestionOptionId_ActivatesReduction.HasValue && questionOptionIds.Contains(red.QuestionOptionId_ActivatesReduction.Value)).ToList();

                applicableReductions.AddRange(potentialReductions.Where(red => red.RegistrableId1_ReductionActivatedIfCombinedWith.HasValue && bookedRegistrableIds.Contains(red.RegistrableId1_ReductionActivatedIfCombinedWith.Value) &&
                                                                               (!red.RegistrableId2_ReductionActivatedIfCombinedWith.HasValue || bookedRegistrableIds.Contains(red.RegistrableId2_ReductionActivatedIfCombinedWith.Value))));

                price -= applicableReductions.Sum(red => red.Amount);
            }

            return price;
        }

        private static async Task SendStatusMail(ICollection<Registrable> registrables, List<Seat> seats, Guid mainRegistrationId)
        {
            // Assumption: Only one Registrable has a limit
            SendMailCommand sendMailCommand = new SendMailCommand
            {
                RegistrationId = mainRegistrationId
            };
            ;
            foreach (var seat in seats)
            {
                var registrable = registrables.FirstOrDefault(rbl => rbl.Id == seat.RegistrableId);
                if (registrable == null)
                {
                    throw new Exception($"No registrable found with Id {seat.RegistrableId}");
                }
                if (registrable.MaximumSingleSeats.HasValue)
                {
                    sendMailCommand.Type = seat.IsWaitingList ? MailType.SingleRegistrationOnWaitingList :
                                                                MailType.SingleRegistrationAccepted;
                    break;
                }
                if (registrable.MaximumDoubleSeats.HasValue)
                {
                    if (seat.RegistrationId.HasValue && seat.RegistrationId_Follower.HasValue)
                    {
                        sendMailCommand.Type = seat.IsWaitingList ? MailType.DoubleRegistrationMatchedOnWaitingList :
                                                                    MailType.DoubleRegistrationMatchedAndAccepted;
                        sendMailCommand.RegistrationId_Partner = seat.RegistrationId == mainRegistrationId ? seat.RegistrationId_Follower :
                                                                                                             seat.RegistrationId;
                    }
                    else
                    {
                        if (seat.PartnerEmail == null)
                        {
                            // single registration for double registrable
                            sendMailCommand.Type = seat.IsWaitingList ? MailType.SingleRegistrationOnWaitingList :
                                                                        MailType.SingleRegistrationAccepted;
                        }
                        else
                        {
                            // partner registration for double registrable
                            sendMailCommand.Type = seat.IsWaitingList ? MailType.DoubleRegistrationFirstPartnerOnWaitingList :
                                                                        MailType.DoubleRegistrationFirstPartnerAccepted;
                        }
                    }
                    sendMailCommand.MainRegistrationRole = seat.RegistrationId == mainRegistrationId ? Role.Leader : Role.Follower;

                    break;
                }
            }
            await ServiceBusClient.SendEvent(sendMailCommand, SendMail.SendMailCommandsQueueName);
        }

        private static async Task<Seat> ReserveSeat(EventRegistratorDbContext context,
                                                    Guid? eventId,
                                                    Registrable registrable,
                                                    Response response,
                                                    string ownEmail,
                                                    string partnerEmail,
                                                    Role? role,
                                                    TraceWriter log)
        {
            Seat seat = null;
            if (registrable.MaximumSingleSeats.HasValue)
            {
                log.Info($"Registrable {registrable.Name}, Seat count {registrable.Seats.Count}");
                seat = new Seat
                {
                    FirstPartnerJoined = DateTime.UtcNow,
                    RegistrationId = response.RegistrationId,
                    RegistrableId = registrable.Id,
                    IsWaitingList = registrable.Seats.Count >= registrable.MaximumSingleSeats.Value
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
                var waitingList = registrable.Seats.Where(st => st.IsWaitingList).ToList();
                if (isPartnerRegistration)
                {
                    // complement existing partner seat
                    var existingPartnerSeat = await FindPartnerSeat(eventId, ownEmail, partnerEmail, ownRole, registrable.Seats, context.Registrations, log);

                    if (existingPartnerSeat != null)
                    {
                        ComplementExistingSeat(response.RegistrationId, ownRole, existingPartnerSeat);
                        return existingPartnerSeat;
                    }

                    // create new partner seat
                    var waitingListForPartnerRegistrations = waitingList.Any(st => !string.IsNullOrEmpty(st.PartnerEmail));
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
                        PartnerEmail = partnerEmail,
                        RegistrationId = ownRole == Role.Leader ? response.RegistrationId : (Guid?)null,
                        RegistrationId_Follower = ownRole == Role.Follower ? response.RegistrationId : (Guid?)null,
                        RegistrableId = registrable.Id,
                        IsWaitingList = waitingListForPartnerRegistrations ||
                                        registrable.Seats.Count >= registrable.MaximumDoubleSeats.Value
                    };
                }
                else
                {
                    // single registration
                    var waitingListForSingleLeaders = waitingList.Any(st => string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId.HasValue);
                    var waitingListForSingleFollowers = waitingList.Any(st => string.IsNullOrEmpty(st.PartnerEmail) && st.RegistrationId_Follower.HasValue);

                    var waitingListForOwnRole = ownRole == Role.Leader && waitingListForSingleLeaders ||
                                                ownRole == Role.Follower && waitingListForSingleFollowers;
                    if (!waitingListForOwnRole)
                    {
                        var matchingSingleSeat = FindMatchingSingleSeat(registrable, ownRole);
                        if (matchingSingleSeat != null)
                        {
                            ComplementExistingSeat(response.RegistrationId, ownRole, matchingSingleSeat);
                            return matchingSingleSeat;
                        }
                    }
                    seat = new Seat
                    {
                        FirstPartnerJoined = DateTime.UtcNow,
                        RegistrationId = ownRole == Role.Leader ? response.RegistrationId : (Guid?)null,
                        RegistrationId_Follower = ownRole == Role.Follower ? response.RegistrationId : (Guid?)null,
                        RegistrableId = registrable.Id,
                        IsWaitingList = waitingListForOwnRole && !CanAddNewDoubleSeat(registrable, ownRole)
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

        private static bool CanAddNewDoubleSeat(Registrable registrable, Role ownRole)
        {
            // check overall
            if (registrable.Seats.Count >= (registrable.MaximumDoubleSeats ?? int.MaxValue))
            {
                return false;
            }
            // check imbalance
            if (!registrable.MaximumAllowedImbalance.HasValue)
            {
                return true;
            }

            if (ownRole == Role.Leader)
            {
                var singleLeaderCount = registrable.Seats.Count(seat => string.IsNullOrEmpty(seat.PartnerEmail) && seat.RegistrationId_Follower == null);
                return singleLeaderCount < registrable.MaximumAllowedImbalance.Value;
            }
            if (ownRole == Role.Leader)
            {
                var singleFollowerCount = registrable.Seats.Count(seat => string.IsNullOrEmpty(seat.PartnerEmail) && seat.RegistrationId == null);
                return singleFollowerCount < registrable.MaximumAllowedImbalance.Value;
            }
            return false;
        }

        private static Seat FindMatchingSingleSeat(Registrable registrable, Role ownRole)
        {
            return registrable.Seats?.FirstOrDefault(st => string.IsNullOrEmpty(st.PartnerEmail) &&
                                                           ownRole == Role.Leader && !st.RegistrationId.HasValue ||
                                                           ownRole == Role.Follower && !st.RegistrationId_Follower.HasValue);
        }

        private static async Task<Seat> FindPartnerSeat(Guid? eventId, string ownEmail, string partnerEmail, Role ownRole, ICollection<Seat> existingSeats, IQueryable<Registration> registrations, TraceWriter log)
        {
            var partnerSeats = existingSeats.Where(seat => seat.PartnerEmail == ownEmail).ToList();
            if (!partnerSeats.Any())
            {
                return null;
            }
            var otherRole = ownRole == Role.Leader ? Role.Follower : Role.Leader;
            var partnerRegistrationIds = partnerSeats.Select(seat => otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower).ToList();
            var partnerRegistrationThatReferenceThisEmail = await registrations.Where(reg => (!eventId.HasValue || reg.RegistrationForm.EventId == eventId.Value) &&
                                                                                             partnerRegistrationIds.Contains(reg.Id))
                                                                               .ToListAsync();
            log.Info($"Partner registrations with this partner mail: {string.Join(", ", partnerRegistrationThatReferenceThisEmail.Select(reg => reg.Id))}");
            var partnerRegistrationId = partnerRegistrationThatReferenceThisEmail.FirstOrDefault(reg => reg.RespondentEmail == partnerEmail)?.Id;
            return partnerSeats.FirstOrDefault(seat => partnerRegistrationId == (otherRole == Role.Leader ? seat.RegistrationId : seat.RegistrationId_Follower));
        }
    }
}