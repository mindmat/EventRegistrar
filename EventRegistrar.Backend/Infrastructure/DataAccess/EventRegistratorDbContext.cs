using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;
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
            builder.ApplyConfiguration(new QuestionMap());
            builder.ApplyConfiguration(new QuestionOptionMap());
            builder.ApplyConfiguration(new QuestionOptionToRegistrableMappingMap());
            builder.ApplyConfiguration(new ResponseMap());

            builder.ApplyConfiguration(new EventMap());
            builder.ApplyConfiguration(new RegistrableMap());
            builder.ApplyConfiguration(new ReductionMap());
            builder.ApplyConfiguration(new RegistrationFormMap());
            builder.ApplyConfiguration(new RegistrationMap());
            builder.ApplyConfiguration(new UserMap());
            builder.ApplyConfiguration(new UserInEventMap());
            builder.ApplyConfiguration(new SeatMap());
            builder.ApplyConfiguration(new AccessToEventRequestMap());
            builder.ApplyConfiguration(new IndividualReductionMap());
            builder.ApplyConfiguration(new RegistrationCancellationMap());

            builder.ApplyConfiguration(new ReceivedPaymentMap());
            builder.ApplyConfiguration(new PaymentAssignmentMap());
            builder.ApplyConfiguration(new PaymentFileMap());

            builder.ApplyConfiguration(new MailMap());
            builder.ApplyConfiguration(new MailToRegistrationMap());
            builder.ApplyConfiguration(new MailTemplateMap());
            builder.ApplyConfiguration(new SmsMap());

            builder.ApplyConfiguration(new RawRegistrationFormMap());
            builder.ApplyConfiguration(new RawRegistrationMap());
        }

        /*
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegistrableComposition>()
                .HasRequired(cmp => cmp.Registrable_Contains)
                .WithMany()
                .HasForeignKey(cmp => cmp.RegistrableId_Contains);

            modelBuilder.Entity<RegistrableComposition>()
                .HasRequired(cmp => cmp.Registrable)
                .WithMany(rbl => rbl.Compositions)
                .HasForeignKey(cmp => cmp.RegistrableId_Contains);
        }
         */
    }
}