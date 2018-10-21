using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations.Overview
{
    public class OverviewController : Controller
    {
        private readonly IMediator _mediator;

        public OverviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/checkinView")]
        public Task<CheckinView> GetCheckinView(string eventAcronym)
        {
            return _mediator.Send(new CheckinQuery { EventAcronym = eventAcronym });
        }
    }
}