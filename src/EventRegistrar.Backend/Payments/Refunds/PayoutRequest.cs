using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Refunds;

public class PayoutRequest : Entity
{
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public IList<PaymentAssignment>? Assignments { get; set; }
    public DateTimeOffset Created { get; set; }
    public PayoutState State { get; set; }
}

public enum PayoutState
{
    Requested = 1,
    Sent = 2,
    Confirmed = 3
}

public class PayoutRequestMap : EntityTypeConfiguration<PayoutRequest>
{
    public override void Configure(EntityTypeBuilder<PayoutRequest> builder)
    {
        base.Configure(builder);
        builder.ToTable("PayoutRequests");

        builder.HasOne(pas => pas.Registration)
               .WithMany()
               .HasForeignKey(pas => pas.RegistrationId);
    }
}