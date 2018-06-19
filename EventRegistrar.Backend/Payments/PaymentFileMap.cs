using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentFileMap : EntityTypeConfiguration<PaymentFile>
    {
        public override void Configure(EntityTypeBuilder<PaymentFile> builder)
        {
            base.Configure(builder);
            builder.ToTable("PaymentFiles");
        }
    }
}