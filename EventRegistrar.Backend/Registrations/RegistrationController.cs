using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations
{
    public class RegistrationController : Controller
    {
        private readonly IMediator _mediator;

        public RegistrationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/registrations")]
        public Task<IEnumerable<RegistrationMatch>> SearchRegistration(string eventAcronym, string searchString)
        {
            return _mediator.Send(new SearchRegistrationQuery { EventAcronym = eventAcronym, SearchString = searchString });
        }
    }
}