using EventRegistrar.Backend.Payments;

namespace EventRegistrar.Backend.Mailing.Compose;

public class PaidAmountSummarizer
{
    private readonly IQueryable<PaymentAssignment> _payments;

    public PaidAmountSummarizer(IQueryable<PaymentAssignment> payments)
    {
        _payments = payments;
    }

    public Task<decimal> GetPaidAmount(Guid registrationId)
    {
        return _payments.Where(pmt => pmt.RegistrationId == registrationId)
                        .SumAsync(pmt => pmt.PayoutRequestId == null ? pmt.Amount : -pmt.Amount);
    }
}