using EventRegistrar.Backend.Payments;

namespace EventRegistrar.Backend.Mailing.Compose;

public class PaidAmountSummarizer(IQueryable<PaymentAssignment> payments)
{
    public Task<decimal> GetPaidAmount(Guid registrationId)
    {
        return payments.Where(pmt => pmt.RegistrationId == registrationId)
                       .SumAsync(pmt => pmt.PayoutRequestId == null ? pmt.Amount : -pmt.Amount);
    }
}