using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class DomainEventDisplayItem
    {
        public DateTimeOffset? Timestamp { get; set; }
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }

    public class DomainEventsQuery : IRequest<IEnumerable<DomainEventDisplayItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class DomainEventsQueryHandler : IRequestHandler<DomainEventsQuery, IEnumerable<DomainEventDisplayItem>>
    {
        private readonly IQueryable<PersistedDomainEvent> _domainEvents;
        private readonly Container _container;

        public DomainEventsQueryHandler(IQueryable<PersistedDomainEvent> domainEvents,
                                        Container container)
        {
            _domainEvents = domainEvents;
            _container = container;
        }

        public async Task<IEnumerable<DomainEventDisplayItem>> Handle(DomainEventsQuery query, CancellationToken cancellationToken)
        {
            var rawEvents = await _domainEvents.Where(evt => evt.EventId == query.EventId)
                                               .OrderByDescending(evt => evt.Sequence)
                                               .Take(100)
                                               .Select(evt => new
                                               {
                                                   evt.Id,
                                                   evt.Timestamp,
                                                   evt.Type,
                                                   evt.Data
                                               })
                                               .ToListAsync(cancellationToken);

            return rawEvents.Select(dev => new DomainEventDisplayItem
            {
                Id = dev.Id,
                Timestamp = dev.Timestamp,
                Type = TranslateType(dev.Type),
                Content = TranslateData(dev.Type, dev.Data)
            });
        }

        private string TranslateType(string type)
        {
            return Resources.ResourceManager.GetString(type.Replace('.', '_')) ?? type;
        }

        private string TranslateData(string type, string data)
        {
            try
            {
                var domainEventType = Type.GetType(type);
                var domainEvent = JsonSerializer.Deserialize(data, domainEventType) as DomainEvent;
                var translationType = typeof(IEventToUserTranslation<>).MakeGenericType(domainEventType);
                //var translationsType = typeof(IEnumerable<>).MakeGenericType(translationType);
                var translation = _container.GetInstance(translationType);
                var method = translationType.GetMethod(nameof(IEventToUserTranslation<DomainEvent>.GetText));
                return method?.Invoke(translation, new object[] { domainEvent }) as string;
            }
            catch
            {
                return data;
            }
        }
    }
}
