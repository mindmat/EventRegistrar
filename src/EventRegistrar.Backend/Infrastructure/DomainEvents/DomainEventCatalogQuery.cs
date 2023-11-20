namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class DomainEventCatalogQuery : IRequest<IEnumerable<DomainEventCatalogItem>> { }

public class
    DomainEventCatalogQueryHandler(DomainEventCatalog catalog) : IRequestHandler<DomainEventCatalogQuery, IEnumerable<DomainEventCatalogItem>>
{
    public Task<IEnumerable<DomainEventCatalogItem>> Handle(DomainEventCatalogQuery request,
                                                            CancellationToken cancellationToken)
    {
        return Task.FromResult(catalog.DomainEventTypes as IEnumerable<DomainEventCatalogItem>);
    }
}