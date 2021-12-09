using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class AccessToEventRequest : Entity
{
    public string? Email { get; set; }
    public Event? Event { get; set; }
    public Guid EventId { get; set; }
    public string? FirstName { get; set; }
    public string? Identifier { get; set; }
    public IdentityProvider IdentityProvider { get; set; }
    public string? LastName { get; set; }
    public DateTime RequestReceived { get; set; }
    public string? RequestText { get; set; }
    public RequestResponse? Response { get; set; }
    public string? ResponseText { get; set; }
    public Guid? UserId_Requestor { get; set; }
    public Guid? UserId_Responder { get; set; }
}

public class AccessToEventRequestMap : EntityTypeConfiguration<AccessToEventRequest>
{
    public override void Configure(EntityTypeBuilder<AccessToEventRequest> builder)
    {
        base.Configure(builder);
        builder.ToTable("AccessToEventRequests");

        builder.HasOne(arq => arq.Event)
               .WithMany(evt => evt.AccessRequests)
               .HasForeignKey(arq => arq.EventId);

        builder.Property(arq => arq.Identifier)
               .HasMaxLength(200);
        builder.Property(arq => arq.FirstName)
               .HasMaxLength(200);
        builder.Property(arq => arq.LastName)
               .HasMaxLength(200);
        builder.Property(arq => arq.Email)
               .HasMaxLength(200);
    }
}