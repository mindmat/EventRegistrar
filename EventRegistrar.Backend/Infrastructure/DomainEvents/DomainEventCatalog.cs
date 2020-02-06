using System;
using System.Collections.Generic;
using System.Linq;

using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class DomainEventCatalog
    {
        public List<DomainEventCatalogItem> DomainEventTypes { get; }

        public DomainEventCatalog(IEnumerable<Type> domainEventTypes)
        {
            DomainEventTypes = domainEventTypes.Select(det => new DomainEventCatalogItem
            {
                TypeName = det.FullName,
                UserText = TranslateType(det.FullName)
            })
                .ToList();
        }
        private string TranslateType(string type)
        {
            return Resources.ResourceManager.GetString(type.Replace('.', '_')) ?? type;
        }
    }

    public class DomainEventCatalogItem
    {
        public string? TypeName { get; set; }
        public string UserText { get; set; }
    }
}