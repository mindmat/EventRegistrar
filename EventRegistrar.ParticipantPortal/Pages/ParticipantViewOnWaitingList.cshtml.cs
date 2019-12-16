using EventRegistrar.ParticipantPortal.RequestHandlers.WaitingList;

using MediatR;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.ParticipantPortal
{
    public class ParticipantViewOnWaitingListModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ParticipantViewOnWaitingListModel(IMediator mediator,
                                                 ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public void OnGet()
        {
            var result = _mediator.Send(new ParticipantViewOnRegistrableQuery { RegistrationId = new System.Guid("5AB848D4-DC51-4EF2-9C11-2B36C3EEE146") });

        }
    }
}