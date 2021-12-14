using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Refunds;

public class PayoutRequest : Entity
{
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    public IList<PaymentAssignment>? Assignments { get; set; }

    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset Created { get; set; }
    public PayoutState State { get; set; }
}

public enum PayoutState
{
    Requested = 1,
    Sent = 2,
    Confirmed = 3
}

public class PayoutRequestMap : EntityMap<PayoutRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PayoutRequest> builder)
    {
        builder.ToTable("PayoutRequests");

        builder.HasOne(pas => pas.Registration)
               .WithMany()
               .HasForeignKey(pas => pas.RegistrationId);
    }
}