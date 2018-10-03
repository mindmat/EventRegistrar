using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.Raw;
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

        [HttpDelete("api/events/{eventAcronym}/registrations/{registrationId:guid}")]
        public Task CancelRegistration(string eventAcronym, Guid registrationId, string reason, bool ignorePayments, decimal refundPercentage)
        {
            return _mediator.Send(new CancelRegistrationCommand { EventAcronym = eventAcronym, RegistrationId = registrationId, Reason = reason, IgnorePayments = ignorePayments, RefundPercentage = refundPercentage });
        }

        [HttpGet("api/registrationforms/{formExternalIdentifier}/RegistrationExternalIdentifiers")]
        public Task<IEnumerable<string>> GetAllExternalRegistrationIdentifiers(string formExternalIdentifier)
        {
            return _mediator.Send(new AllExternalRegistrationIdentifiersQuery { RegistrationFormExternalIdentifier = formExternalIdentifier });
        }

        [HttpGet("api/events/{eventAcronym}/registrations")]
        public Task<IEnumerable<RegistrationMatch>> SearchRegistration(string eventAcronym, string searchString, IEnumerable<RegistrationState> states)
        {
            return _mediator.Send(new SearchRegistrationQuery { EventAcronym = eventAcronym, SearchString = searchString, States = states });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}")]
        public Task<RegistrationDisplayItem> SearchRegistration(string eventAcronym, Guid registrationId)
        {
            return _mediator.Send(new RegistrationQuery { EventAcronym = eventAcronym, RegistrationId = registrationId });
        }
    }
}