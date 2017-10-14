using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.RegistrationForms;
using EventRegistrator.Functions.Registrations;
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
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Response> Responses { get; set; }

        public DbSet<DomainEvent> DomainEvents { get; set; }

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

            modelBuilder.Entity<QuestionOption>()
                .HasRequired(qop => qop.Question)
                .WithMany(qst => qst.QuestionOptions)
                .HasForeignKey(qop => qop.QuestionId);

            base.OnModelCreating(modelBuilder);
        }
    }
}