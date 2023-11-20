using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Bulk;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing.Templates;

public class MailTemplatesController(IMediator mediator,
                                     IEventAcronymResolver eventAcronymResolver)
    : Controller
{
    [HttpDelete("api/events/{eventAcronym}/mailTemplates/{mailTemplateId:guid}")]
    public async Task DeleteMailTemplate(string eventAcronym, Guid mailTemplateId)
    {
        await mediator.Send(new DeleteBulkMailTemplateCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                MailTemplateId = mailTemplateId
                            });
    }
}