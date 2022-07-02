namespace EventRegistrar.Backend.Registrations;

public class RegistrationDisplayItem
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsReduced { get; set; }
    public bool? IsWaitingList { get; set; }
    public string? Language { get; set; }
    public decimal Paid { get; set; }
    public Guid? PartnerId { get; set; }
    public string? PartnerOriginal { get; set; }
    public string? PartnerName { get; set; }
    public string? PhoneNormalized { get; set; }
    public string? PhoneFormatted { get; internal set; }
    public decimal? Price { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Remarks { get; set; }
    public int ReminderLevel { get; set; }
    public int SmsCount { get; set; }
    public string? SoldOutMessage { get; set; }
    public RegistrationState Status { get; set; }
    public string StatusText { get; set; }
    public bool WillPayAtCheckin { get; set; }
    public bool? FallbackToPartyPass { get; set; }
}