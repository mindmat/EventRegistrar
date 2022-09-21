using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Mailing.Templates;

public class MailTemplatesController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public MailTemplatesController(IMediator mediator,
                                   IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpDelete("api/events/{eventAcronym}/mailTemplates/{mailTemplateId:guid}")]
    public async Task DeleteMailTemplate(string eventAcronym, Guid mailTemplateId)
    {
        await _mediator.Send(new DeleteMailTemplateCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 MailTemplateId = mailTemplateId
                             });
    }
}