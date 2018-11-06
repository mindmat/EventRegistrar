using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Infrastructure.Events
{
    public class SaveDomainEventCommandHandler : IRequestHandler<SaveDomainEventCommand>
    {
        private readonly DbSet<DomainEvent> _domainEvents;

        public SaveDomainEventCommandHandler(DbContext dbContext)
        {
            _domainEvents = dbContext.Set<DomainEvent>();
        }

        public async Task<Unit> Handle(SaveDomainEventCommand command, CancellationToken cancellationToken)
        {
            await _domainEvents.AddAsync(new DomainEvent
            {
                Id = Guid.NewGuid(),
                EventId = null,
                Timestamp = DateTime.UtcNow,
                Type = command.EventType,
                Data = command.EventData
            }, cancellationToken);

            return Unit.Value;
        }
    }
}