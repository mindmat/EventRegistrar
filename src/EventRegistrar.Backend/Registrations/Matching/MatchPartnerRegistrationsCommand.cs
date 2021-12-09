using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Spots;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Matching;

public class MatchPartnerRegistrationsCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId1 { get; set; }
    public Guid RegistrationId2 { get; set; }
}

public class MatchPartnerRegistrationsCommandHandler : IRequestHandler<MatchPartnerRegistrationsCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly IRepository<Seat> _seats;
    private readonly ServiceBusClient _serviceBusClient;

    public MatchPartnerRegistrationsCommandHandler(IRepository<Registration> registrations,
                                                   IRepository<Seat> seats,
                                                   ServiceBusClient serviceBusClient)
    {
        _registrations = registrations;
        _seats = seats;
        _serviceBusClient = serviceBusClient;
    }

    public async Task<Unit> Handle(MatchPartnerRegistrationsCommand command, CancellationToken cancellationToken)
    {
        var registration1 = await _registrations.Where(reg => reg.EventId == command.EventId
                                                           && reg.Id == command.RegistrationId1)
                                                .Include(reg => reg.Seats_AsLeader)
                                                .ThenInclude(seat => seat.Registrable)
                                                .Include(reg => reg.Seats_AsFollower)
                                                .ThenInclude(seat => seat.Registrable)
                                                .FirstAsync(cancellationToken);
        var registration2 = await _registrations.Where(reg => reg.EventId == command.EventId
                                                           && reg.Id == command.RegistrationId2)
                                                .Include(reg => reg.Seats_AsLeader)
                                                .ThenInclude(seat => seat.Registrable)
                                                .Include(reg => reg.Seats_AsFollower)
                                                .ThenInclude(seat => seat.Registrable)
                                                .FirstAsync(cancellationToken);

        if (registration1.RegistrationId_Partner.HasValue)
            throw new ArgumentException($"Registration {registration1.Id} is already assigned to a partner");
        if (registration2.RegistrationId_Partner.HasValue)
            throw new ArgumentException($"Registration {registration2.Id} is already assigned to a partner");

        var registrationLeader = registration1;
        var registrationFollower = registration2;
        if (registrationLeader.Seats_AsFollower.Any(seat =>
                !seat.IsCancelled && seat.Registrable.MaximumDoubleSeats.HasValue))
        {
            registrationLeader = registration2;
            registrationFollower = registration1;
        }

        if (registrationLeader.Seats_AsFollower.Any(seat =>
                !seat.IsCancelled && seat.Registrable.MaximumDoubleSeats.HasValue))
            throw new ArgumentException(
                $"Unexpected situation: leader registration {registrationLeader.Id} has partner spots as follower");
        if (registrationFollower.Seats_AsLeader.Any(seat =>
                !seat.IsCancelled && seat.Registrable.MaximumDoubleSeats.HasValue))
            throw new ArgumentException(
                $"Unexpected situation: follower registration {registrationFollower.Id} has partner spots as leader");

        var partnerSpotsOfLeader = registrationLeader.Seats_AsLeader
                                                     .Where(seat =>
                                                         !seat.IsCancelled && seat.Registrable.MaximumDoubleSeats
                                                             .HasValue)
                                                     .ToList();
        var partnerSpotsOfFollower = registrationFollower.Seats_AsFollower
                                                         .Where(seat =>
                                                             !seat.IsCancelled && seat.Registrable.MaximumDoubleSeats
                                                                 .HasValue)
                                                         .ToList();

        if (partnerSpotsOfLeader.Any(spt => spt.RegistrationId_Follower.HasValue))
            throw new ArgumentException(
                $"Unexpected situation: leader registration {registrationLeader.Id} has partner spot with a follower set");
        if (partnerSpotsOfFollower.Any(spt => spt.RegistrationId.HasValue))
            throw new ArgumentException(
                $"Unexpected situation: follower registration {registrationFollower.Id} has partner spot with a leader set");

        // ok, everything seems to be fine, let's match
        registrationLeader.RegistrationId_Partner = registrationFollower.Id;
        registrationFollower.RegistrationId_Partner = registrationLeader.Id;

        var registrableIdsToMerge = partnerSpotsOfLeader.Select(spt => spt.RegistrableId)
                                                        .Union(partnerSpotsOfFollower.Select(spt => spt.RegistrableId))
                                                        .Distinct()
                                                        .ToList();

        var isWaitingList = false;
        foreach (var registrableId in registrableIdsToMerge)
        {
            var leaderSpot = partnerSpotsOfLeader.Single(spt => spt.RegistrableId == registrableId);
            var followerSpot = partnerSpotsOfFollower.Single(spt => spt.RegistrableId == registrableId);
            Seat mergedSpot;
            Seat spotToCancel;
            if (leaderSpot.FirstPartnerJoined < followerSpot.FirstPartnerJoined ||
                !leaderSpot.IsWaitingList && followerSpot.IsWaitingList)
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

            await _seats.InsertOrUpdateEntity(mergedSpot, cancellationToken);
            await _seats.InsertOrUpdateEntity(spotToCancel, cancellationToken);
            isWaitingList |= mergedSpot.IsWaitingList;
        }

        // update waiting list
        registration1.IsWaitingList = registration1.Seats_AsFollower.Any(spt => !spt.IsCancelled && spt.IsWaitingList)
                                   || registration1.Seats_AsLeader.Any(spt => !spt.IsCancelled && spt.IsWaitingList);
        registration2.IsWaitingList = registration2.Seats_AsFollower.Any(spt => !spt.IsCancelled && spt.IsWaitingList)
                                   || registration2.Seats_AsLeader.Any(spt => !spt.IsCancelled && spt.IsWaitingList);

        var mailType = isWaitingList
            ? MailType.PartnerRegistrationMatchedOnWaitingList
            : MailType.PartnerRegistrationMatchedAndAccepted;
        _serviceBusClient.SendMessage(new ComposeAndSendMailCommand
                                      {
                                          EventId = command.EventId,
                                          RegistrationId = registrationLeader.Id,
                                          //Withhold = true,
                                          MailType = mailType
                                      });

        return Unit.Value;
    }
}