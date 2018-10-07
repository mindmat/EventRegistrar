using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrar.Backend.Mailing.Send
{
    public class SendMailCommandHandler : IRequestHandler<SendMailCommand>
    {
        private const string MessageIdHeader = "X-Message-Id";
        private readonly ILogger _logger;
        private readonly IRepository<Mail> _mails;

        public SendMailCommandHandler(ILogger logger,
                                      IRepository<Mail> mails)
        {
            _logger = logger;
            _mails = mails;
        }

        public async Task<Unit> Handle(SendMailCommand command, CancellationToken cancellationToken)
        {
            var mail = await _mails.FirstAsync(mil => mil.Id == command.MailId, cancellationToken);
            var msg = new SendGridMessage
            {
                From = new SendGrid.Helpers.Mail.EmailAddress(command.Sender?.Email, command.Sender?.Name),
                Subject = command.Subject,
                PlainTextContent = command.ContentPlainText,
                HtmlContent = command.ContentHtml,
            };

            msg.AddTos(command.To.Select(to => new SendGrid.Helpers.Mail.EmailAddress(to.Email, to.Name)).ToList());

            // send mail
            var apiKey = Environment.GetEnvironmentVariable("SendGrid_ApiKey");
            var client = new SendGridClient(apiKey);

            var response = await client.SendEmailAsync(msg, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
            {
                _logger.LogWarning($"ComposeAndSendMailCommandHandler status {response.StatusCode}, Body {await response.Body.ReadAsStringAsync()}");
            }
            else
            {
                if (response.Headers.TryGetValues(MessageIdHeader, out var values))
                {
                    mail.SendGridMessageId = values.FirstOrDefault();
                }
                else
                {
                    _logger.LogWarning($"Header {MessageIdHeader} not present in response, cannot determine Message-ID");
                }
            }

            return Unit.Value;
        }
    }
}