namespace EventRegistrar.Backend.PhoneMessages;

public class SmsDisplayItem
{
    public string Body { get; set; }
    public DateTimeOffset? Date { get; set; }
    public bool Sent { get; set; }
    public string Status { get; set; }
}