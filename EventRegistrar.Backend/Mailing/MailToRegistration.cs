using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing
{
    public class MailToRegistration : Entity
    {
        public Mail Mail { get; set; }
        public Guid MailId { get; set; }
        public Registration Registration { get; set; }
        public Guid RegistrationId { get; set; }
        public MailState? State { get; set; }
    }
}