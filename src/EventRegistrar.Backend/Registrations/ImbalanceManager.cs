using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations;

public class ImbalanceManager
{
    public bool CanAddNewDoubleSpotForSingleRegistration(int maximumDoubleSeat,
                                                         int maximumAllowedImbalance,
                                                         IList<Seat> spots,
                                                         Role? role)
    {
        // check overall
        if (spots.Count(seat => seat is { IsWaitingList: false, IsCancelled: false }) >= maximumDoubleSeat)
        {
            return false;
        }

        // check imbalance
        if (role == Role.Leader)
        {
            var acceptedSingleLeaderCount = spots.Count(spt => spt is { IsWaitingList: false, IsCancelled: false }
                                                            && string.IsNullOrEmpty(spt.PartnerEmail)
                                                            && spt.RegistrationId_Follower == null);
            return acceptedSingleLeaderCount < maximumAllowedImbalance;
        }

        if (role == Role.Follower)
        {
            var acceptedSingleFollowerCount = spots.Count(spt => spt is { IsWaitingList: false, IsCancelled: false }
                                                              && string.IsNullOrEmpty(spt.PartnerEmail)
                                                              && spt.RegistrationId == null);
            return acceptedSingleFollowerCount < maximumAllowedImbalance;
        }

        return false;
    }
}