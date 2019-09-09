using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class ReleaseMailCommandHandler : IRequestHandler<ReleaseMailCommand>
    {
        private readonly IRepository<Mail> _mails;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IEventBus _eventBus;

        public ReleaseMailCommandHandler(IRepository<Mail> mails,
                                         ServiceBusClient serviceBusClient,
                                         IEventBus eventBus)
        {
            _mails = mails;
            _serviceBusClient = serviceBusClient;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(ReleaseMailCommand command, CancellationToken cancellationToken)
        {
            var withheldMail = await _mails
                                     .Where(mail => mail.Id == command.MailId)
                                     .Include(mail => mail.Registrations).ThenInclude(map => map.Registration)
                                     .FirstAsync(cancellationToken);
            if (withheldMail.Discarded)
            {
                throw new ArgumentException($"Mail {withheldMail.Id} is discarded and thus cannot be sent");
            }

            var sendMailCommand = new SendMailCommand
            {
                MailId = withheldMail.Id,
                ContentHtml = withheldMail.ContentHtml,
                ContentPlainText = withheldMail.ContentPlainText,
                Subject = withheldMail.Subject,
                Sender = new EmailAddress { Email = withheldMail.SenderMail, Name = withheldMail.SenderName },
                To = withheldMail.Registrations
                                 .GroupBy(reg => reg.Registration.RespondentEmail?.ToLowerInvariant())
                                 .Select(grp => new EmailAddress
                                 {
                                     Email = grp.Key,
                                     Name = grp.Select(reg => reg.Registration.RespondentFirstName).StringJoin(" & ") // avoid ',' obviously SendGrid interprets commas
                                 }).ToList()
            };

            withheldMail.Withhold = false;
            withheldMail.Sent = DateTime.UtcNow;

            _serviceBusClient.SendMessage(sendMailCommand);

            _eventBus.Publish(new MailReleased
            {
                MailId = withheldMail.Id,
                To = sendMailCommand.To.Select(to => $"{to.Name} - {to.Email}").StringJoin()
            });

            return Unit.Value;
        }
    }
}