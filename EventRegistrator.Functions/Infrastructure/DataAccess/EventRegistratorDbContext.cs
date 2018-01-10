using System.Data.Entity;
using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Payments;
using EventRegistrator.Functions.Registrables;
using EventRegistrator.Functions.RegistrationForms;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Seats;

namespace EventRegistrator.Functions.Infrastructure.DataAccess
{
    public class EventRegistratorDbContext : DbContext
    {
        public EventRegistratorDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Reduction> Reductions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<RegistrationForm> RegistrationForms { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<MailTemplate> MailTemplates { get; set; }
        public DbSet<ReceivedPayment> ReceivedPayments { get; set; }

        public DbSet<DomainEvent> DomainEvents { get; set; }
        public DbSet<Registrable> Registrables { get; set; }
        public DbSet<QuestionOptionToRegistrableMapping> QuestionOptionToRegistrableMappings { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<MailToRegistration> MailToRegistrations { get; set; }
        public DbSet<PaymentFile> PaymentFiles { get; set; }
        public DbSet<PaymentAssignment> PaymentAssignments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Question>()
            //    .HasRequired(qst => qst.RegistrationForm)
            //    .WithMany(frm => frm.Questions)
            //    .HasForeignKey(qst => qst.RegistrationFormId);

            modelBuilder.Entity<RegistrationForm>()
                .HasRequired(frm => frm.Event)
                .WithMany()
                .HasForeignKey(frm => frm.EventId);

            modelBuilder.Entity<Registration>()
                .HasRequired(frm => frm.RegistrationForm)
                .WithMany()
                .HasForeignKey(frm => frm.RegistrationFormId);

            modelBuilder.Entity<Response>()
                .HasOptional(frm => frm.Question)
                .WithMany()
                .HasForeignKey(frm => frm.QuestionId);

            //modelBuilder.Entity<QuestionOption>()
            //    .HasRequired(qop => qop.Question)
            //    .WithMany(qst => qst.QuestionOptions)
            //    .HasForeignKey(qop => qop.QuestionId);

            modelBuilder.Entity<QuestionOptionToRegistrableMapping>()
                .HasRequired(qop => qop.Registrable)
                .WithMany(qst => qst.QuestionOptionMappings)
                .HasForeignKey(qop => qop.RegistrableId);

            modelBuilder.Entity<Seat>()
                .HasRequired(seat => seat.Registrable)
                .WithMany(rbl => rbl.Seats)
                .HasForeignKey(seat => seat.RegistrableId);

            modelBuilder.Entity<Seat>()
                .HasOptional(seat => seat.Registration)
                .WithMany()
                .HasForeignKey(seat => seat.RegistrationId);

            modelBuilder.Entity<Seat>()
                .HasOptional(seat => seat.Registration_Follower)
                .WithMany()
                .HasForeignKey(seat => seat.RegistrationId_Follower);

            modelBuilder.Entity<ReceivedPayment>()
                .HasRequired(pmt => pmt.PaymentFile)
                .WithMany()
                .HasForeignKey(pmt => pmt.PaymentFileId);

            modelBuilder.Entity<PaymentAssignment>()
                .HasRequired(pas => pas.ReceivedPayment)
                .WithMany(pmt => pmt.Assignments)
                .HasForeignKey(pas => pas.ReceivedPaymentId);

            modelBuilder.Entity<PaymentAssignment>()
                .HasRequired(pas => pas.Registration)
                .WithMany(pmt => pmt.Payments)
                .HasForeignKey(pas => pas.RegistrationId);

            modelBuilder.Entity<MailToRegistration>()
                .HasRequired(map => map.Mail)
                .WithMany(mail => mail.Registrations)
                .HasForeignKey(map => map.MailId);

            modelBuilder.Entity<MailToRegistration>()
                .HasRequired(map => map.Registration)
                .WithMany(reg => reg.Mails)
                .HasForeignKey(map => map.RegistrationId);

            base.OnModelCreating(modelBuilder);
        }
    }
}