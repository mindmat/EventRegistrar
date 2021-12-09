namespace EventRegistrar.Backend.Payments.Files.Camt;

public class CamtFile
{
    public string Account { get; set; }
    public decimal Balance { get; set; }
    public DateTime? BookingsFrom { get; set; }
    public DateTime? BookingsTo { get; set; }
    public string Currency { get; set; }
    public IReadOnlyCollection<CamtEntry> Entries { get; set; }
    public string FileId { get; set; }
    public string Owner { get; set; }
}