using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Files.Fetch;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files;

public class PaymentFileController(IMediator mediator,
                                   IEventAcronymResolver eventAcronymResolver) : Controller
{
    [HttpPost("api/events/{eventAcronym}/paymentfiles/upload")]
    public async Task UploadFile(string eventAcronym, IFormFile file)
    {
        var stream = new MemoryStream((int)file.Length);
        await file.CopyToAsync(stream);
        await mediator.Send(new SavePaymentFileCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                FileStream = stream,
                                Filename = file.FileName,
                                ContentType = file.ContentType
                            });
    }


    [HttpPost("api/events/{eventAcronym}/fetchBankStatementFiles")]
    public async Task FetchBankStatementFiles(string eventAcronym)
    {
        await mediator.Send(new FetchBankStatementsFileCommand
                            {
                                EventId = await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                            });
    }
}