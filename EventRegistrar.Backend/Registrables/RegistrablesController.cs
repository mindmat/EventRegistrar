using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrablesController : Controller
    {
        private readonly IMediator _mediator;

        public RegistrablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/SingleRegistrableOverview")]
        public Task<IEnumerable<SingleRegistrableDisplayItem>> GetSingleRegistrablesOverivew(string eventAcronym)
        {
            return _mediator.Send(new SingleRegistrablesOverviewQuery { EventAcronym = eventAcronym });
        }
    }
}