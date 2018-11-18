using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class PartnerMatchingController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public PartnerMatchingController(IMediator mediator,
                                         IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/changeToSingleRegistration")]
        public async Task ChangeOpenPartnerRegistrationToSingleRegistration(string eventAcronym, Guid registrationId)
        {
            await _mediator.Send(new ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/potentialMatches")]
        public async Task<IEnumerable<PotentialPartnerMatch>> GetPotentialPartnerMatches(string eventAcronym, Guid registrationId, string searchString)
        {
            return await _mediator.Send(new PotentialPartnersQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId = registrationId,
                SearchString = searchString
            });
        }

        [HttpGet("api/events/{eventAcronym}/registrations/unmatchedPartners")]
        public async Task<IEnumerable<PotentialPartnerMatch>> GetRegistrationsWithUnmatchedPartner(string eventAcronym)
        {
            return await _mediator.Send(new RegistrationsWithUnmatchedPartnerQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });
        }

        [HttpPost("api/events/{eventAcronym}/registrations/matchPartners")]
        public async Task MatchPartners(string eventAcronym, Guid registrationId1, Guid registrationId2)
        {
            await _mediator.Send(new MatchPartnerRegistrationsCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                RegistrationId1 = registrationId1,
                RegistrationId2 = registrationId2
            });
        }
    }
}