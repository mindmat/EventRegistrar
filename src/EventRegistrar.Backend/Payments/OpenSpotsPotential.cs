namespace EventRegistrar.Backend.Payments;

public class OpenSpotsPotential
{
    public string Name { get; set; }
    public decimal PotentialIncome { get; set; }
    public Guid RegistrableId { get; set; }
    public int SpotsAvailable { get; set; }
}