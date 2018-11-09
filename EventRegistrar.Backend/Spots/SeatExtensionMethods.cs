using System;

namespace EventRegistrar.Backend.Spots
{
    public static class SeatExtensionMethods
    {
        public static Guid? GetOtherRegistrationId(this Seat seat, Guid registrationId)
        {
            if (seat.RegistrationId == registrationId)
            {
                return seat.RegistrationId_Follower;
            }
            if (seat.RegistrationId_Follower == registrationId)
            {
                return seat.RegistrationId;
            }

            return null;
        }
    }
}