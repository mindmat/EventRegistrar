using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Events
{
    public static class EventsExtensionMethods
    {
        public static async Task<Guid> GetEventId(this IQueryable<Event> events, string eventAcronym)
        {
            return (await events.FirstAsync(evt => evt.Acronym == eventAcronym)).Id;
        }
    }
}