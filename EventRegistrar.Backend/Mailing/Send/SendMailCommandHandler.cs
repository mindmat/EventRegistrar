using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrar.Backend.Mailing.Send
{
    public class SendMailCommandHandler : IRequestHandler<SendMailCommand>
    {
        private readonly ILogger _logger;

        public SendMailCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(SendMailCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"C# ServiceBus queue trigger function processed message: {command}");

            var msg = new SendGridMessage
            {
                From = new SendGrid.Helpers.Mail.EmailAddress(command.Sender?.Email, command.Sender?.Name),
                Subject = command.Subject,
                PlainTextContent = command.ContentPlainText,
                HtmlContent = command.ContentHtml
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

            return Unit.Value;
        }
    }
}