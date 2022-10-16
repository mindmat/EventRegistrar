using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class AccessToEventRequest : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public Guid? UserId_Requestor { get; set; }
    public User? User_Requestor { get; set; }
    public Guid? UserId_Responder { get; set; }
    public User? User_Responder { get; set; }

    public string? Identifier { get; set; }
    public IdentityProvider IdentityProvider { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }

    public DateTimeOffset RequestReceived { get; set; }
    public string? RequestText { get; set; }
    public RequestResponse? Response { get; set; }
    public string? ResponseText { get; set; }
}

public class AccessToEventRequestMap : EntityMap<AccessToEventRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AccessToEventRequest> builder)
    {
        builder.ToTable("AccessToEventRequests");

        builder.HasOne(arq => arq.Event)
               .WithMany(evt => evt.AccessRequests)
               .HasForeignKey(arq => arq.EventId);

        builder.HasOne(arq => arq.User_Requestor)
               .WithMany()
               .HasForeignKey(arq => arq.UserId_Requestor);

        builder.HasOne(arq => arq.User_Responder)
               .WithMany()
               .HasForeignKey(arq => arq.UserId_Responder);

        builder.Property(arq => arq.Identifier)
               .HasMaxLength(200);
        builder.Property(arq => arq.FirstName)
               .HasMaxLength(200);
        builder.Property(arq => arq.LastName)
               .HasMaxLength(200);
        builder.Property(arq => arq.Email)
               .HasMaxLength(200);
        builder.Property(arq => arq.AvatarUrl)
               .HasMaxLength(500);
    }
}