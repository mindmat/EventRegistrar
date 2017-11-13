using System;

namespace EventRegistrator.Functions.Mailing
{
    public class ComposeAndSendMailCommand
    {
        public Guid? RegistrationId { get; set; }
        public bool Withhold { get; set; }
    }
}