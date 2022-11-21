using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Mailing.InvalidAddresses;
using EventRegistrar.Backend.Mailing.ManualTrigger;

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

    [HttpDelete("api/events/{eventAcronym}/mails/{mailId:guid}")]
    public async Task DeleteMail(string eventAcronym, Guid mailId)
    {
        await _mediator.Send(new DeleteMailCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 MailId = mailId
                             });
    }

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

    [HttpGet("api/events/{eventAcronym}/invalidMailAddresses")]
    public async Task<IEnumerable<InvalidAddress>> GetInvalidMailAddresses(string eventAcronym)
    {
        return await _mediator.Send(new InvalidAddressesQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/mails")]
    public async Task<IEnumerable<MailDisplayItem>> GetMailsOfRegistration(string eventAcronym, Guid registrationId)
    {
        return await _mediator.Send(new MailsOfRegistrationQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        RegistrationId = registrationId
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/mails/notreceived")]
    public async Task<IEnumerable<NotReceivedMail>> GetNotReceivedMails(string eventAcronym)
    {
        return await _mediator.Send(new NotReceivedMailsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/registrations/{registrationId:guid}/possibleMailTypes")]
    public async Task<IEnumerable<MailTypeItem>> GetPossibleMailTypes(string eventAcronym, Guid registrationId)
    {
        return await _mediator.Send(new PossibleMailTypesQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        RegistrationId = registrationId
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