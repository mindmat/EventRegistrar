using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Seats;
using EventRegistrar.Backend.Users;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public class EventRegistratorDbContext : DbContext
    {
        public EventRegistratorDbContext(DbContextOptions<EventRegistratorDbContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new RegistrableMap());
            builder.ApplyConfiguration(new RegistrationFormMap());
            builder.ApplyConfiguration(new RegistrationMap());
            builder.ApplyConfiguration(new UserMap());
            builder.ApplyConfiguration(new UserInEventMap());
            builder.ApplyConfiguration(new SeatMap());
        }

        /*
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
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
                .WithMany(rbl => rbl.Compositions)
                .HasForeignKey(cmp => cmp.RegistrableId_Contains);

            base.OnModelCreating(modelBuilder);
        }
         */
    }
}