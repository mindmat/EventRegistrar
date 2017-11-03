using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Mailing
{
    public class MailToRegistration : Entity
    {
        public Guid MailId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}