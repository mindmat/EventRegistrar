using System;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Events
{
    public interface IEventAcronymResolver
    {
        Task<Guid> GetEventIdFromAcronym(string eventAcronym);
    }
}