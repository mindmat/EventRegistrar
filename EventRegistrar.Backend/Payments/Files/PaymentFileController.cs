using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files
{
    public class PaymentFileController : Controller
    {
        private readonly IMediator _mediator;

        public PaymentFileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/events/{eventAcronym}/paymentfiles/upload")]
        public async Task UploadFile(string eventAcronym, IFormFile file)
        {
            var stream = new MemoryStream((int)file.Length);
            await file.CopyToAsync(stream);
            await _mediator.Send(new SavePaymentFileCommand { EventAcronym = eventAcronym, FileStream = stream });
        }
    }
}