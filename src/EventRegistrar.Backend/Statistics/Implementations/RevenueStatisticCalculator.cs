using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Statistics.Implementations;

/// <summary>
/// Calculates the total revenue (payments received) per event.
/// </summary>
public class RevenueStatisticCalculator(IQueryable<PaymentAssignment> paymentAssignments)
    : IStatisticCalculator
{
    public string Key => "EventRevenue";
    public string DisplayName => "Event Revenue";
    public string Category => "Financial";
    public string Unit => "CHF";

    /// <summary>
    /// Calculate revenue statistics for multiple events at once.
    /// </summary>
    public async Task<IDictionary<Guid, decimal>> CalculateBatch(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
    {
        return await paymentAssignments.Where(pas => eventIds.Contains(pas.Registration!.EventId)
                                                  && pas.Registration!.State != RegistrationState.Cancelled)
                                       .GroupBy(pas => pas.Registration!.EventId)
                                       .ToDictionaryAsync(pas => pas.Key,
                                                          pas => pas.Sum(asn => asn.OutgoingPayment == null
                                                                                    ? asn.Amount
                                                                                    : -asn.Amount),
                                                          cancellationToken);
    }
}