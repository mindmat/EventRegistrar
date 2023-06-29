using System.Net;

using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Payments.Due;

using MailKit.Net.Smtp;

using MimeKit;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrar.Backend.Mailing.Send;

public class SendMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string ContentHtml { get; set; }
    public string ContentPlainText { get; set; }
    public Guid MailId { get; set; }
    public EmailAddress Sender { get; set; }
    public string Subject { get; set; }
    public IEnumerable<EmailAddress> To { get; set; }
}

public class SendMailCommandHandler : AsyncRequestHandler<SendMailCommand>
{
    private const string MessageIdHeader = "X-Message-Id";
    private readonly ILogger _logger;
    private readonly IRepository<Mail> _mails;
    private readonly ReadModelUpdater _readModelUpdater;
    private readonly MailConfiguration _mailConfiguration;
    private readonly ExternalMailConfigurations _externalMailConfiguration;

    public SendMailCommandHandler(ILogger logger,
                                  IRepository<Mail> mails,
                                  ReadModelUpdater readModelUpdater,
                                  MailConfiguration mailConfiguration,
                                  ExternalMailConfigurations externalMailConfiguration)
    {
        _logger = logger;
        _mails = mails;
        _readModelUpdater = readModelUpdater;
        _mailConfiguration = mailConfiguration;
        _externalMailConfiguration = externalMailConfiguration;
    }

    protected override async Task Handle(SendMailCommand command, CancellationToken cancellationToken)
    {
        var mail = await _mails.AsTracking()
                               .FirstAsync(mil => mil.Id == command.MailId, cancellationToken);
        if (_mailConfiguration.MailSender == MailSender.Imap)
        {
            var config = _externalMailConfiguration.MailConfigurations?.FirstOrDefault();
            if (config != null
             && mail.SenderMail != null
             && config.ImapHost != null)
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(mail.SenderName, mail.SenderMail));
                foreach (var mailAddress in command.To)
                {
                    mailMessage.To.Add(new MailboxAddress(mailAddress.Name, mailAddress.Email));
                }

                mailMessage.Subject = mail.Subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = mail.ContentHtml };
                mailMessage.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(config.ImapHost, config.ImapPort, true, cancellationToken);
                await smtpClient.AuthenticateAsync(config.Username, config.Password, cancellationToken);
                await smtpClient.SendAsync(mailMessage, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);
            }
        }
        else
        {
            var msg = new SendGridMessage
                      {
                          From = new SendGrid.Helpers.Mail.EmailAddress(command.Sender?.Email, command.Sender?.Name),
                          Subject = command.Subject,
                          PlainTextContent = command.ContentPlainText,
                          HtmlContent = command.ContentHtml,
                          CustomArgs = new Dictionary<string, string>
                                       { { nameof(SendGridEvent.MailId), mail.Id.ToString() } }
                      };

            msg.AddTos(command.To.Select(to => new SendGrid.Helpers.Mail.EmailAddress(to.Email, to.Name)).ToList());

            // send mail
            var apiKey = Environment.GetEnvironmentVariable("SendGrid_ApiKey");
            var client = new SendGridClient(apiKey);

            var response = await client.SendEmailAsync(msg, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
            {
                _logger.LogWarning($"ComposeAndSendMailCommandHandler status {response.StatusCode}, Body {await response.Body.ReadAsStringAsync(cancellationToken)}");
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
        }

        if (mail.EventId != null)
        {
            _readModelUpdater.TriggerUpdate<DuePaymentsCalculator>(null, mail.EventId);
        }
    }
}