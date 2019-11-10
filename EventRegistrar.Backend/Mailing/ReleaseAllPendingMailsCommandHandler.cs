using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class ReleaseAllPendingMailsCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class ReleaseAllPendingMailsCommandHandler : IRequestHandler<ReleaseAllPendingMailsCommand>
    {
        private readonly IRepository<Mail> _mails;
        private readonly ServiceBusClient _serviceBusClient;

        public ReleaseAllPendingMailsCommandHandler(IRepository<Mail> mails,
                                                    ServiceBusClient serviceBusClient)
        {
            _mails = mails;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(ReleaseAllPendingMailsCommand command, CancellationToken cancellationToken)
        {
            var withheldMails = await _mails
                                      .Where(mail => mail.EventId == command.EventId
                                                  && !mail.Discarded
                                                  && mail.State == null
                                                  && mail.Sent == null)
                                      .Include(mail => mail.Registrations).ThenInclude(map => map.Registration)
                                      .OrderByDescending(mail => mail.Created)
                                      .ToListAsync(cancellationToken);
            foreach (var withheldMail in withheldMails)
            {
                _serviceBusClient.SendMessage(new ReleaseMailCommand { EventId = command.EventId, MailId = withheldMail.Id });
            }
            return Unit.Value;
        }
    }
}