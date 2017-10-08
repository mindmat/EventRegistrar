using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.RegistrationForms;
using System.Data.Entity;

namespace EventRegistrator.Functions.Infrastructure.DataAccess
{
    public class EventRegistratorDbContext : DbContext
    {
        public EventRegistratorDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<RegistrationForm> RegistrationForms { get; set; }

        public DbSet<Question> Questions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>()
                .HasRequired(qst => qst.RegistrationForm)
                .WithMany(frm => frm.Questions)
                .HasForeignKey(qst => qst.RegistrationFormId);

            modelBuilder.Entity<RegistrationForm>()
                .HasRequired(frm => frm.Event)
                .WithMany()
                .HasForeignKey(frm => frm.EventId);

            base.OnModelCreating(modelBuilder);
        }
    }
}