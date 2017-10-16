using System.Collections.Generic;

namespace EventRegistrator.Functions.Mailing
{
    public class SendMailCommand
    {
        public IEnumerable<MailAddress> Recipients { get; set; }
    }
}