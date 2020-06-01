using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class RawMailEvents : Entity
    {
        public string Body { get; set; } = null!;
        public DateTime Created { get; set; }
        public DateTime? Processed { get; set; }
    }

    public class RawMailEventsMap : EntityTypeConfiguration<RawMailEvents>
    {
    }
}