using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.RegistrationForms;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrator.Functions.Infrastructure.DataAccess
{
    public class EventRegistratorDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }

        public DbSet<RegistrationForm> RegistrationForms { get; set; }
        //public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var @event = modelBuilder.Entity<Event>();

            @event.HasKey(evt => evt.Id);
            @event.Property(evt => evt.RowVersion)
                .IsRowVersion();

            var registrationForm = modelBuilder.Entity<RegistrationForm>();
            registrationForm.HasKey(frm => frm.Id);
            registrationForm.Property(frm => frm.RowVersion)
                .IsRowVersion();

            registrationForm.HasOne(frm => frm.Event)
                .WithMany()
                .HasForeignKey(frm => frm.EventId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=tcp:eventregistrator.database.windows.net,1433;Initial Catalog=EventRegistrator;Persist Security Info=False;User ID=mindmat;Password='A%~tX,aT3&cu}~vbv)Y.m?&y';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            base.OnConfiguring(optionsBuilder);
        }
    }
}