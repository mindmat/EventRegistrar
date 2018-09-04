using System;

namespace EventRegistrar.Backend.Spots
{
    public class Spot
    {
        public DateTime FirstPartnerJoined { get; set; }
        public Guid Id { get; set; }
        public bool IsCore { get; set; }
        public string Partner { get; set; }
        public Guid? PartnerRegistrationId { get; set; }
        public string Registrable { get; set; }
        public Guid RegistrableId { get; set; }
        public int? SortKey { get; set; }
    }
}