using System.Data.Entity;
using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Payments;
using EventRegistrator.Functions.Registrables;
using EventRegistrator.Functions.RegistrationForms;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Registrations.Cancellation;
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
        public DbSet<RegistrationCancellation> RegistrationCancellations { get; set; }
        public DbSet<Sms.Sms> Sms { get; set; }
        public DbSet<RegistrableComposition> RegistrableCompositions { get; set; }

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
                .HasRequired(reg => reg.RegistrationForm)
                .WithMany()
                .HasForeignKey(reg => reg.RegistrationFormId);

            modelBuilder.Entity<Sms.Sms>()
                .HasOptional(sms => sms.Registration)
                .WithMany(reg => reg.Sms)
                .HasForeignKey(sms => sms.RegistrationId);

            modelBuilder.Entity<Response>()
                .HasRequired(rsp => rsp.Registration)
                .WithMany()
                .HasForeignKey(rsp => rsp.RegistrationId);

            modelBuilder.Entity<Response>()
                .HasOptional(rsp => rsp.Question)
                .WithMany()
                .HasForeignKey(rsp => rsp.QuestionId);

            modelBuilder.Entity<Reduction>()
               .HasRequired(red => red.Registrable)
               .WithMany(rbl => rbl.Reductions)
               .HasForeignKey(rsp => rsp.RegistrableId);

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
                .WithMany(reg => reg.Seats_AsLeader)
                .HasForeignKey(seat => seat.RegistrationId);

            modelBuilder.Entity<Seat>()
                .HasOptional(seat => seat.Registration_Follower)
                .WithMany(reg => reg.Seats_AsFollower)
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

            modelBuilder.Entity<RegistrableComposition>()
                .HasRequired(cmp => cmp.Registrable_Contains)
                .WithMany()
                .HasForeignKey(cmp => cmp.RegistrableId_Contains);

            modelBuilder.Entity<RegistrableComposition>()
                .HasRequired(cmp => cmp.Registrable)
                .WithMany(rbl=>rbl.Compositions)
                .HasForeignKey(cmp => cmp.RegistrableId_Contains);

            base.OnModelCreating(modelBuilder);
        }
    }
}