using System;

namespace EventRegistrator.Functions.WaitingList
{
    public class TryPromoteFromWaitingListCommand
    {
        public Guid EventId { get; set; }
        public Guid? RegistrableId { get; set; }
    }
}