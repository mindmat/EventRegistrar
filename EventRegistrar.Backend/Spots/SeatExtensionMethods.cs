using System;
using EventRegistrar.Backend.Registrations;

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

        public static Role GetSingleRole(this Seat spot)
        {
            if (spot.IsPartnerSpot || spot.PartnerEmail != null)
            {
                throw new ArgumentException($"Spot {spot.Id} is not a single seat");
            }
            if (spot.RegistrationId == null && spot.RegistrationId_Follower == null)
            {
                throw new ArgumentException($"Unexpected situation: Spot {spot.Id} has neither leader nor follower set");
            }
            if (spot.RegistrationId != null && spot.RegistrationId_Follower != null)
            {
                throw new ArgumentException($"Unexpected situation: Spot {spot.Id} has leader and follower set");
            }

            return spot.RegistrationId != null
                   ? Role.Leader
                   : Role.Follower;
        }

        public static bool IsSingleFollowerSpot(this Seat spot)
        {
            return spot.RegistrationId == null
                   && spot.RegistrationId_Follower != null
                   && !spot.IsPartnerSpot
                   && spot.PartnerEmail == null;
        }

        public static bool IsSingleLeaderSpot(this Seat spot)
        {
            return spot.RegistrationId != null
                && spot.RegistrationId_Follower == null
                && !spot.IsPartnerSpot
                && spot.PartnerEmail == null;
        }

        public static void MergeSingleSpots(this Seat spotTarget, Seat spotSource)
        {
            if (spotTarget.IsSingleLeaderSpot() && spotSource.IsSingleFollowerSpot())
            {
                spotTarget.RegistrationId_Follower = spotSource.RegistrationId_Follower;
                spotSource.IsCancelled = true;
                spotTarget.IsWaitingList &= spotSource.IsWaitingList;
            }
            else if (spotTarget.IsSingleFollowerSpot() && spotSource.IsSingleLeaderSpot())
            {
                spotTarget.RegistrationId = spotSource.RegistrationId;
                spotSource.IsCancelled = true;
                spotTarget.IsWaitingList &= spotSource.IsWaitingList;
            }
            else
            {
                throw new ArgumentException($"Unexpected situation: spots {spotTarget.Id} and {spotSource.Id} cannot be merged");
            }
        }
    }
}