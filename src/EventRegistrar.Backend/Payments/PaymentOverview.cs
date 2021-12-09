namespace EventRegistrar.Backend.Payments;

public class PaymentOverview
{
    public BalanceDto Balance { get; set; }
    public int NotFullyPaidRegistrations { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int PaidRegistrations { get; set; }
    public IEnumerable<OpenSpotsPotential> PotentialOfOpenSpots { get; set; }
    public decimal ReceivedMoney { get; set; }
}