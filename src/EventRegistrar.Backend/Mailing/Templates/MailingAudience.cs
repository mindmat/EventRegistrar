namespace EventRegistrar.Backend.Mailing.Templates;

[Flags]
public enum MailingAudience
{
    Paid = 1,
    Unpaid = 2,
    WaitingList = 4,
    PredecessorEvent = 8,
    PrePredecessorEvent = 16
}