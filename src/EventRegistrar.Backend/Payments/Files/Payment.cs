using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files.Slips;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files;

public class Payment : Entity
{
    public Guid PaymentsFileId { get; set; }
    public PaymentsFile? PaymentsFile { get; set; }

    public string? Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal? Charges { get; set; }
    public DateTime BookingDate { get; set; }


    public string? Info { get; set; }
    public string? Message { get; set; }
    public string? InstructionIdentification { get; set; }

    public string? RawXml { get; set; }
    public string? RecognizedEmail { get; set; }
    public string? Reference { get; set; }
    public decimal? Repaid { get; set; }
    public bool Settled { get; set; }
    public bool Ignore { get; set; }
    public IncomingPayment? Incoming { get; set; }
    public OutgoingPayment? Outgoing { get; set; }

    public PaymentType Type { get; set; }
}

public enum PaymentType
{
    Incoming = 1,
    Outgoing = 2
}

public class PaymentMap : EntityMap<Payment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasOne(pmt => pmt.PaymentsFile)
               .WithMany()
               .HasForeignKey(pmt => pmt.PaymentsFileId);

        builder.Property(pmt => pmt.Info)
               .HasMaxLength(400);

        builder.Property(pmt => pmt.Reference)
               .HasMaxLength(100);

        builder.Property(pmt => pmt.RecognizedEmail)
               .HasMaxLength(100);

        builder.Property(pmt => pmt.InstructionIdentification)
               .HasMaxLength(200);
    }
}

public class OutgoingPayment : Entity
{
    public Payment? Payment { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public ICollection<PaymentAssignment>? Assignments { get; set; }
}

public class OutgoingPaymentMap : EntityMap<OutgoingPayment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OutgoingPayment> builder)
    {
        builder.ToTable("OutgoingPayments");

        builder.HasOne(pmo => pmo.Payment)
               .WithOne(pmt => pmt.Outgoing)
               .HasForeignKey<OutgoingPayment>(pmo => pmo.Id);

        builder.Property(pmo => pmo.CreditorName)
               .HasMaxLength(500);

        builder.Property(pmo => pmo.CreditorIban)
               .HasMaxLength(50);
    }
}

public class IncomingPayment : Entity
{
    public Payment? Payment { get; set; }
    public string? DebitorIban { get; set; }
    public string? DebitorName { get; set; }
    public Guid? PaymentSlipId { get; set; }
    public PaymentSlip? PaymentSlip { get; set; }
    public ICollection<PaymentAssignment>? Assignments { get; set; }
}

public class IncomingPaymentMap : EntityMap<IncomingPayment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IncomingPayment> builder)
    {
        builder.ToTable("IncomingPayments");

        builder.HasOne(pmi => pmi.Payment)
               .WithOne(pmt => pmt.Incoming)
               .HasForeignKey<IncomingPayment>(pmi => pmi.Id);

        builder.HasOne(pmi => pmi.PaymentSlip)
               .WithMany()
               .HasForeignKey(pmi => pmi.PaymentSlipId);

        builder.Property(pmi => pmi.DebitorName)
               .HasMaxLength(500);

        builder.Property(pmi => pmi.DebitorIban)
               .HasMaxLength(50);
    }
}