using System.Linq;

namespace EventRegistrator.Functions.Registrables
{
    public static class ImbalanceManager
    {
        public static bool CanAddNewDoubleSeatForSingleRegistration(Registrable registrable, Role ownRole)
        {
            // check overall
            if (registrable.Seats.Count(seat => !seat.IsWaitingList && !seat.IsCancelled) >= (registrable.MaximumDoubleSeats ?? int.MaxValue))
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
                var acceptedSingleLeaderCount = registrable.Seats.Count(seat => !seat.IsWaitingList && !seat.IsCancelled && string.IsNullOrEmpty(seat.PartnerEmail) && seat.RegistrationId_Follower == null);
                return acceptedSingleLeaderCount < registrable.MaximumAllowedImbalance.Value;
            }
            if (ownRole == Role.Follower)
            {
                var acceptedSingleFollowerCount = registrable.Seats.Count(seat => !seat.IsWaitingList && !seat.IsCancelled && string.IsNullOrEmpty(seat.PartnerEmail) && seat.RegistrationId == null);
                return acceptedSingleFollowerCount < registrable.MaximumAllowedImbalance.Value;
            }
            return false;
        }
    }
}