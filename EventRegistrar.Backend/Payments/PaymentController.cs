using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentController : Controller
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/events/{eventAcronym}/payments/overview")]
        public Task<PaymentOverview> GetPaymentOverview(string eventAcronym)
        {
            return _mediator.Send(new PaymentOverviewQuery { EventAcronym = eventAcronym });
        }
    }
}