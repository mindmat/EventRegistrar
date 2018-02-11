using System;

namespace EventRegistrator.Functions.Mailing
{
    [Flags]
    public enum MailingAudience
    {
        Paid = 1,
        Unpaid = 2,
        WaitingList = 4
    }
}