using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events
{
    public interface IEventAcronymResolver
    {
        Task<Guid> GetEventIdFromAcronym(string eventAcronym);
    }

    internal class EventAcronymResolver : IEventAcronymResolver
    {
        private readonly IQueryable<Event> _events;

        public EventAcronymResolver(IQueryable<Event> events)
        {
            _events = events;
        }

        public async Task<Guid> GetEventIdFromAcronym(string eventAcronym)
        {
            var @event = await _events.FirstOrDefaultAsync(evt => evt.Acronym == eventAcronym);
            if (@event == null)
            {
                throw new ArgumentOutOfRangeException($"There is no event {eventAcronym}");
            }

            return @event.Id;
        }
    }
}