using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Import;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing;

public class MailsController(IMediator mediator,
                             IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpPost("api/events/{eventAcronym}/mails/import")]
    public async Task ImportMails(string eventAcronym)
    {
        await mediator.Send(new ImportMailsFromImapCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                            });
    }
}