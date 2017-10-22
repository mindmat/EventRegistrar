using System;

namespace EventRegistrator.Functions.Mailing
{
    public class SendMailCommand
    {
        public MailType Type { get; set; }
        public Guid? RegistrationId { get; set; }
        public Guid? RegistrationId_Follower { get; set; }
    }
}