using System;

namespace EventRegistrar.Backend.Spots
{
    public class SpotRemover
    {
        public void RemoveSpot(Seat place, Guid registrationId)
        {
            if (place.RegistrationId == registrationId)
            {
                if (place.RegistrationId_Follower.HasValue)
                {
                    // double place, leave the partner in
                    place.RegistrationId = null;
                    place.PartnerEmail = null;
                }
                else
                {
                    // single place, cancel the place
                    place.IsCancelled = true;
                }
            }
            else if (place.RegistrationId_Follower == registrationId)
            {
                if (place.RegistrationId.HasValue)
                {
                    // double place, leave the partner in
                    place.RegistrationId_Follower = null;
                    place.PartnerEmail = null;
                }
                else
                {
                    // single place, cancel the place
                    place.IsCancelled = true;
                }
            }
        }
    }
}