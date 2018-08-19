using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
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

        public ReleaseMailCommandHandler(IRepository<Mail> mails,
                                         ServiceBusClient serviceBusClient)
        {
            _mails = mails;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(ReleaseMailCommand command, CancellationToken cancellationToken)
        {
            var withheldMail = await _mails
                                     .Where(mail => mail.Id == command.MailId)
                                     .Include(mail => mail.Registrations).ThenInclude(map => map.Registration)
                                     .FirstOrDefaultAsync(cancellationToken);
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
            return Unit.Value;
        }
    }
}