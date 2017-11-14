using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;

namespace EventRegistrator.Functions.Mailing
{
    public class MailToRegistration : Entity
    {
        public Guid MailId { get; set; }
        public Guid RegistrationId { get; set; }
        public Mail Mail { get; set; }
        public Registration Registration { get; set; }
    }
}