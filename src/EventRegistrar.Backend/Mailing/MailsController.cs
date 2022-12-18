using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Mailing.InvalidAddresses;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing;

public class MailsController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public MailsController(IMediator mediator,
                           IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    //[HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/mails/create")]
    //public async Task CreateMailForRegistration(string eventAcronym,
    //                                            Guid registrationId,
    //                                            MailType? mailType,
    //                                            string bulkMailKey,
    //                                            bool withhold,
    //                                            bool allowDuplicate)
    //{
    //    await _mediator.Send(new ComposeAndSendMailCommand
    //                         {
    //                             EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
    //                             RegistrationId = registrationId,
    //                             MailType = mailType,
    //                             BulkMailKey = bulkMailKey,
    //                             Withhold = true,
    //                             AllowDuplicate = allowDuplicate
    //                         });
    //}


    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/fixEmailAddress")]
    public async Task FixInvalidMailAddress(string eventAcronym,
                                            Guid registrationId,
                                            string oldEmailAddress,
                                            string newEmailAddress)
    {
        await _mediator.Send(new FixInvalidAddressCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrationId = registrationId,
                                 OldEmailAddress = oldEmailAddress,
                                 NewEmailAddress = newEmailAddress
                             });
    }


    [HttpPost("api/events/{eventAcronym}/mails/import")]
    public async Task ImportMails(string eventAcronym)
    {
        await _mediator.Send(new ImportMailsFromImapCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                             });
    }

    [HttpGet("api/events/{eventAcronym}/mails/possibleAudiences")]
    public async Task<IEnumerable<PossibleAudience>> GetPossibleAudiences(string eventAcronym)
    {
        return await _mediator.Send(new PossibleAudiencesQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }
}