using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class DomainEventCatalogQuery : IRequest<IEnumerable<DomainEventCatalogItem>>
    {

    }

    public class DomainEventCatalogQueryHandler : IRequestHandler<DomainEventCatalogQuery, IEnumerable<DomainEventCatalogItem>>
    {
        private readonly DomainEventCatalog _catalog;

        public DomainEventCatalogQueryHandler(DomainEventCatalog catalog)
        {
            _catalog = catalog;
        }

        public Task<IEnumerable<DomainEventCatalogItem>> Handle(DomainEventCatalogQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_catalog.DomainEventTypes as IEnumerable<DomainEventCatalogItem>);
        }
    }
}