using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Spots
{
    public class SpotRemover
    {
        private readonly EventBus _eventBus;

        public SpotRemover(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void RemoveSpot(Seat spot, Guid registrationId)
        {
            if (spot.RegistrationId == registrationId)
            {
                if (spot.RegistrationId_Follower.HasValue)
                {
                    // double spot, leave the partner in
                    spot.RegistrationId = null;
                    spot.PartnerEmail = null;
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
                }
                else
                {
                    // single spot, cancel the place
                    spot.IsCancelled = true;
                }
            }
            _eventBus.Publish(new SpotRemoved { RegistrableId = spot.RegistrableId, RegistrationId = registrationId });
        }
    }
}