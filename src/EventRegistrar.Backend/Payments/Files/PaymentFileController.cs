using System.IO;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Files.Fetch;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files
{
    public class PaymentFileController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public PaymentFileController(IMediator mediator,
                                     IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpPost("api/events/{eventAcronym}/paymentfiles/upload")]
        public async Task UploadFile(string eventAcronym, IFormFile file)
        {
            var stream = new MemoryStream((int)file.Length);
            await file.CopyToAsync(stream);
            await _mediator.Send(new SavePaymentFileCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                FileStream = stream,
                Filename = file.FileName,
                ContentType = file.ContentType
            });
        }


        [HttpPost("api/events/{eventAcronym}/fetchBankStatementFiles")]
        public async Task FetchBankStatementFiles(string eventAcronym)
        {
            await _mediator.Send(new FetchBankStamentsFileCommand
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }
    }
}