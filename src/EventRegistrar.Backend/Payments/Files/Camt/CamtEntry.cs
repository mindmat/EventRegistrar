namespace EventRegistrar.Backend.Payments.Files.Camt;

public class CamtEntry
{
    public decimal Amount { get; set; }
    public DateTime BookingDate { get; set; }
    public decimal? Charges { get; set; }
    public string? Currency { get; set; }
    public string? DebitorIban { get; set; }
    public string? DebitorName { get; set; }
    public string? Info { get; set; }
    public string? Message { get; set; }
    public string? InstructionIdentification { get; set; }
    public string? Reference { get; set; }
    public CreditDebit Type { get; set; }
    public string? Xml { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public string? DebitorStreet { get; set; }
    public string? DebitorBuildingNr { get; set; }
    public string? DebitorZip { get; set; }
    public string? DebitorTown { get; set; }
    public string? DebitorCountry { get; set; }
    public string? DebitorAdressLine { get; set; }
}