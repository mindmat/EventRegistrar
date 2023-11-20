using EventRegistrar.Backend.Events;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.PhoneMessages;

public class SmsController(IMediator mediator,
                           IEventAcronymResolver eventAcronymResolver)
{
    [HttpGet("api/events/{eventAcronym}/registrations/{registrationid:guid}/sms")]
    public async Task<IEnumerable<SmsDisplayItem>> GetSmsConversation(string eventAcronym, Guid registrationId)
    {
        return await mediator.Send(new SmsConversationQuery
                                   {
                                       EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                       RegistrationId = registrationId
                                   });
    }

    [HttpPost("api/events/{eventAcronym}/registrations/{registrationid:guid}/sms/send")]
    public async Task SendSms(string eventAcronym, Guid registrationId, [FromBody] SmsContent message)
    {
        await mediator.Send(new SendSmsCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                RegistrationId = registrationId,
                                Message = message?.Body
                            });
    }

    public class SmsContent
    {
        public string Body { get; set; }
    }
}