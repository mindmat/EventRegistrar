using System.Collections.Generic;

namespace EventRegistrator.Functions.Seats
{
    public class RegistrableDisplayInfo
    {
        public string Name { get; set; }
        public int? MaximumSingleSeats { get; set; }
        public int? MaximumDoubleSeats { get; set; }
        public int? MaximumAllowedImbalance { get; set; }
        public IEnumerable<PlaceDisplayInfo> Participants { get; set; }
        public bool HasWaitingList { get; set; }
    }
}