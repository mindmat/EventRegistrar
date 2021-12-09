namespace EventRegistrar.Backend.PhoneMessages;

public class SmsDisplayItem
{
    public string Body { get; set; }
    public DateTime? Date { get; set; }
    public bool Sent { get; set; }
    public string Status { get; set; }
}