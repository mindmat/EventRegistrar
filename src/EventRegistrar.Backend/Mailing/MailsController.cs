using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Import;

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


    [HttpPost("api/events/{eventAcronym}/mails/import")]
    public async Task ImportMails(string eventAcronym)
    {
        await _mediator.Send(new ImportMailsFromImapCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                             });
    }
}