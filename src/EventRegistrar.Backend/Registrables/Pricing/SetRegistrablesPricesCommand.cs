namespace EventRegistrar.Backend.Registrables.Pricing;

public class SetRegistrablesPricesCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public decimal? Price { get; set; }
    public decimal? ReducedPrice { get; set; }
}

public class SetRegistrablesPricesCommandHandler(IQueryable<Registrable> registrables) : IRequestHandler<SetRegistrablesPricesCommand>
{
    public async Task Handle(SetRegistrablesPricesCommand command, CancellationToken cancellationToken)
    {
        var registrable = await registrables.FirstAsync(rbl => rbl.EventId == command.EventId
                                                            && rbl.Id == command.RegistrableId,
                                                        cancellationToken);
        registrable.Price = command.Price;
        registrable.ReducedPrice = command.ReducedPrice;
    }
}