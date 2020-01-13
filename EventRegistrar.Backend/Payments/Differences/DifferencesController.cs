using System.Collections.Generic;
using System.Threading.Tasks;

using EventRegistrar.Backend.Events;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Differences
{
    public class DifferencesController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public DifferencesController(IMediator mediator,
                                IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/differences")]
        public async Task<IEnumerable<DifferencesDisplayItem>> GetRefunds(string eventAcronym)
        {
            return await _mediator.Send(new DifferencesQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }
    }
}