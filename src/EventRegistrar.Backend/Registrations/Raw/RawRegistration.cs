using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Raw
{
    public class RawRegistration : Entity
    {
        public DateTime Created { get; set; }
        public string? EventAcronym { get; set; }
        public string? FormExternalIdentifier { get; set; }
        public DateTime? Processed { get; set; }
        public string ReceivedMessage { get; set; } = null!;
        public string RegistrationExternalIdentifier { get; set; } = null!;
    }

    public class RawRegistrationMap : EntityTypeConfiguration<RawRegistration>
    {
        public override void Configure(EntityTypeBuilder<RawRegistration> builder)
        {
            base.Configure(builder);
            builder.ToTable("RawRegistrations");
        }
    }
}