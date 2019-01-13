using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Spots
{
    public class SpotRemover
    {
        private readonly IEventBus _eventBus;

        public SpotRemover(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void RemoveSpot(Seat spot, Guid registrationId, RemoveSpotReason reason)
        {
            if (spot.RegistrationId == registrationId)
            {
                if (spot.RegistrationId_Follower.HasValue)
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
                if (spot.RegistrationId.HasValue)
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
            _eventBus.Publish(new SpotRemoved
            {
                Id = Guid.NewGuid(),
                RegistrableId = spot.RegistrableId,
                RegistrationId = registrationId,
                Reason = reason,
                WasSpotOnWaitingList = spot.IsWaitingList
            });
        }
    }
}