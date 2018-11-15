using System.Collections.Generic;
using System.Linq;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations
{
    public class ImbalanceManager
    {
        public bool CanAddNewDoubleSeatForSingleRegistration(int maximumDoubleSeat,
                                                             int maximumAllowedImbalance,
                                                             IList<Seat> spots,
                                                             Role ownRole)
        {
            // check overall
            if (spots.Count(seat => !seat.IsWaitingList && !seat.IsCancelled) >= maximumDoubleSeat)
            {
                return false;
            }

            // check imbalance
            if (ownRole == Role.Leader)
            {
                var acceptedSingleLeaderCount = spots.Count(spt => !spt.IsWaitingList
                                                                && !spt.IsCancelled
                                                                && string.IsNullOrEmpty(spt.PartnerEmail)
                                                                && spt.RegistrationId_Follower == null);
                return acceptedSingleLeaderCount < maximumAllowedImbalance;
            }
            if (ownRole == Role.Follower)
            {
                var acceptedSingleFollowerCount = spots.Count(spt => !spt.IsWaitingList
                                                                  && !spt.IsCancelled
                                                                  && string.IsNullOrEmpty(spt.PartnerEmail)
                                                                  && spt.RegistrationId == null);
                return acceptedSingleFollowerCount < maximumAllowedImbalance;
            }
            return false;
        }
    }
}