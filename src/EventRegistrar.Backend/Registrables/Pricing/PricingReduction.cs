namespace EventRegistrar.Backend.Registrables.Pricing;

public class PricingReduction
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid? RegistrableId1_ReductionActivatedIfCombinedWith { get; set; }
    public Guid? RegistrableId2_ReductionActivatedIfCombinedWith { get; set; }
}