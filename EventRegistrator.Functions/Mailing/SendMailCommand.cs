using System;
using EventRegistrator.Functions.Registrables;

namespace EventRegistrator.Functions.Mailing
{
    public class SendMailCommand
    {
        public MailType Type { get; set; }
        public Guid? RegistrationId { get; set; }
        public Guid? RegistrationId_Partner { get; set; }
        public Role? MainRegistrationRole { get; set; }
    }
}