using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class ReleaseBulkMailsCommandHandler : IRequestHandler<ReleaseBulkMailsCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IRepository<Mail> _mails;
        private readonly ServiceBusClient _serviceBusClient;

        public ReleaseBulkMailsCommandHandler(IRepository<Mail> mails,
                                              IEventAcronymResolver acronymResolver,
                                              ServiceBusClient serviceBusClient)
        {
            _mails = mails;
            _acronymResolver = acronymResolver;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(ReleaseBulkMailsCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);

            var withheldMails = await _mails
                                      .Where(mail => mail.MailTemplate.BulkMailKey == command.BulkMailKey
                                                  && mail.EventId == eventId
                                                  && mail.Withhold)
                                      .Include(mail => mail.Registrations).ThenInclude(map => map.Registration)
                                      .ToListAsync(cancellationToken);
            foreach (var withheldMail in withheldMails)
            {
                var sendMailCommand = new SendMailCommand
                {
                    MailId = withheldMail.Id,
                    ContentHtml = withheldMail.ContentHtml,
                    ContentPlainText = withheldMail.ContentPlainText,
                    Subject = withheldMail.Subject,
                    Sender = new EmailAddress { Email = withheldMail.SenderMail, Name = withheldMail.SenderName },
                    To = withheldMail.Registrations.Select(reg => new EmailAddress
                    {
                        Email = reg.Registration.RespondentEmail,
                        Name = reg.Registration.RespondentFirstName
                    }).ToList()
                };

                withheldMail.Withhold = false;

                await _serviceBusClient.SendCommand(sendMailCommand);
            }

            return Unit.Value;
        }
    }
}