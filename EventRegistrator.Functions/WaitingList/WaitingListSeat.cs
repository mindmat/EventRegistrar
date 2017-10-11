using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.WaitingList
{
    public class WaitingListSeat : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}