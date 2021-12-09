namespace EventRegistrar.Backend.Registrables.Pricing;

public class RegistrablePricing
{
    public Guid RegistrableId { get; set; }
    public decimal? Price { get; set; }
    public decimal? ReducedPrice { get; set; }
    public string RegistrableName { get; internal set; }
    public IEnumerable<PricingReduction> Reductions { get; set; }
}