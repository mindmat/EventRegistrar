using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events;

public class Event : Entity
{
    public Guid? PredecessorEventId { get; set; }
    public Event? PredecessorEvent { get; set; }

    public ICollection<AccessToEventRequest>? AccessRequests { get; set; }
    public ICollection<EventConfiguration>? Configurations { get; set; }
    public ICollection<Registrable>? Registrables { get; set; }
    public ICollection<Registration>? Registrations { get; set; }
    public ICollection<UserInEvent>? Users { get; set; }

    public string Name { get; set; } = null!;
    public State State { get; set; }
    public string Acronym { get; set; } = null!;
    public string? Currency { get; set; }
    public string? AccountIban { get; set; }
}

public class EventMap : EntityMap<Event>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasOne(evt => evt.PredecessorEvent)
               .WithMany()
               .HasForeignKey(evt => evt.PredecessorEventId);
    }
}