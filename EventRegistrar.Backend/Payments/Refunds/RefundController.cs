using System.Collections.Generic;
using System.Threading.Tasks;

using EventRegistrar.Backend.Events;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Refunds
{
    public class RefundController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public RefundController(IMediator mediator,
                                IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/refunds")]
        public async Task<IEnumerable<RefundDisplayItem>> GetRefunds(string eventAcronym)
        {
            return await _mediator.Send(new RefundsQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }
    }
}