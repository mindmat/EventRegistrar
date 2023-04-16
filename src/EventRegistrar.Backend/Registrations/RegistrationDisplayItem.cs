using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationDisplayItem
{
    public Guid Id { get; set; }
    public string? ReadableId { get; set; }
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
    public DateTimeOffset ReceivedAt { get; set; }
    public string? Remarks { get; set; }
    public bool RemarksProcessed { get; set; }
    public int ReminderLevel { get; set; }
    public int SmsCount { get; set; }
    public string? SoldOutMessage { get; set; }
    public RegistrationState Status { get; set; }
    public bool WillPayAtCheckin { get; set; }
    public bool? FallbackToPartyPass { get; set; }
    public string? InternalNotes { get; set; }
    public Guid? PricePackageId_ManualFallback { get; set; }

    public IEnumerable<SpotDisplayItem>? Spots { get; set; }
    public IEnumerable<AssignedPaymentDisplayItem>? Payments { get; set; }
    public IEnumerable<MailDisplayItem>? Mails { get; set; }
    public IEnumerable<MailDisplayItem>? ImportedMails { get; set; }
    public IEnumerable<IndividualReductionDisplayItem>? Reductions { get; set; }
}

public class MailDisplayItem
{
    public MailDisplayType Type { get; set; }
    public Guid MailId { get; set; }
    public string? Subject { get; set; }
    public MailState? State { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool Withhold { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}

public enum MailDisplayType
{
    Auto = 1,
    Bulk = 2,
    Imported = 3
}

public class IndividualReductionDisplayItem
{
    public Guid Id { get; set; }
    public IndividualReductionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}