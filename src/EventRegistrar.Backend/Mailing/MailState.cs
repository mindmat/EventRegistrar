namespace EventRegistrar.Backend.Mailing;

public enum MailState
{
    Unknown = 0,
    Processed = 1,
    Dropped = 2,
    Delivered = 3,
    Deferred = 4,
    Bounce = 5,
    Open = 6,
    Click = 7,
    SpamReport = 8,
    Unsubscribe = 9,
    GroupUnsubscribe = 10,
    GroupResubscribe = 11
}