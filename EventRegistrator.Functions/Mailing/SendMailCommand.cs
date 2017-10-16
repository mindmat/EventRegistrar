using System;
using System.Collections.Generic;

namespace EventRegistrator.Functions.Mailing
{
    public class SendMailCommand
    {
        public IEnumerable<MailAddress> Recipients { get; set; }
        public MailType Type { get; set; }
        public Guid? RegistrationId { get; set; }
        public Guid? RegistrationId_Follower { get; set; }
    }
}