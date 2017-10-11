using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Seats
{
    public class Seat : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid RegistrationId_Partner { get; set; }
    }
}