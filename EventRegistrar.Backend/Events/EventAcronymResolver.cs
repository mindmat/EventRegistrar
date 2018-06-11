using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events
{
    internal class EventAcronymResolver : IEventAcronymResolver
    {
        private readonly IQueryable<Event> _events;

        public EventAcronymResolver(IQueryable<Event> events)
        {
            _events = events;
        }

        public async Task<Guid> GetEventIdFromAcronym(string eventAcronym)
        {
            var @event = await _events.FirstAsync(evt => evt.Acronym == eventAcronym);
            return @event.Id;
        }
    }
}