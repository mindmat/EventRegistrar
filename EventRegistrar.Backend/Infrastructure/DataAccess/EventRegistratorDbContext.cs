using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Compositions;
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
            builder.ApplyConfiguration(new RegistrableCompositionMap());
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
            builder.ApplyConfiguration(new PaymentSlipMap());

            builder.ApplyConfiguration(new MailMap());
            builder.ApplyConfiguration(new MailToRegistrationMap());
            builder.ApplyConfiguration(new MailTemplateMap());
            builder.ApplyConfiguration(new RawMailEventsMap());
            builder.ApplyConfiguration(new MailEventMap());
            builder.ApplyConfiguration(new SmsMap());

            builder.ApplyConfiguration(new RawRegistrationFormMap());
            builder.ApplyConfiguration(new RawRegistrationMap());

            builder.ApplyConfiguration(new EventConfigurationMap());

            builder.ApplyConfiguration(new PersistedDomainEventMap());
        }
    }
}