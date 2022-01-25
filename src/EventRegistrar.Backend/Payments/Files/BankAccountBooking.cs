using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files.Camt;
using EventRegistrar.Backend.Payments.Files.Slips;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files;

public class BankAccountBooking : Entity
{
    public Guid BankAccountStatementsFileId { get; set; }
    public BankAccountStatementsFile? BankAccountStatementsFile { get; set; }
    public Guid? PaymentSlipId { get; set; }
    public PaymentSlip? PaymentSlip { get; set; }

    public ICollection<PaymentAssignment>? Assignments { get; set; }
    public ICollection<PaymentAssignment>? RepaymentAssignments { get; set; }

    public decimal Amount { get; set; }
    public DateTime BookingDate { get; set; }
    public decimal? Charges { get; set; }
    public CreditDebit? CreditDebitType { get; set; }
    public string? Currency { get; set; }
    public string? DebitorIban { get; set; }
    public string? DebitorName { get; set; }
    public string? Info { get; set; }
    public string? InstructionIdentification { get; set; }

    public string? RawXml { get; set; }
    public string? RecognizedEmail { get; set; }
    public string? Reference { get; set; }
    public decimal? Repaid { get; set; }
    public bool Settled { get; set; }
    public bool Ignore { get; set; }
    public string? Message { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
}

public class AccountStatementMap : EntityMap<BankAccountBooking>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BankAccountBooking> builder)
    {
        builder.ToTable("BankAccountBookings");

        builder.HasOne(pmt => pmt.BankAccountStatementsFile)
               .WithMany()
               .HasForeignKey(pmt => pmt.BankAccountStatementsFileId);

        builder.HasOne(pmt => pmt.PaymentSlip)
               .WithMany()
               .HasForeignKey(pmt => pmt.PaymentSlipId);

        builder.Property(pmt => pmt.Info)
               .HasMaxLength(400);

        builder.Property(pmt => pmt.Reference)
               .HasMaxLength(100);

        builder.Property(pmt => pmt.RecognizedEmail)
               .HasMaxLength(100);

        builder.Property(pmt => pmt.DebitorName)
               .HasMaxLength(200);

        builder.Property(pmt => pmt.DebitorIban)
               .HasMaxLength(200);

        builder.Property(pmt => pmt.InstructionIdentification)
               .HasMaxLength(200);
    }
}