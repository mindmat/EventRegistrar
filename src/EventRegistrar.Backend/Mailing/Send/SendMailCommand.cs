using System.Net;

using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Payments.Due;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrar.Backend.Mailing.Send;

public class SendMailCommand : IRequest
{
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

    public SendMailCommandHandler(ILogger logger,
                                  IRepository<Mail> mails,
                                  ReadModelUpdater readModelUpdater)
    {
        _logger = logger;
        _mails = mails;
        _readModelUpdater = readModelUpdater;
    }

    protected override async Task Handle(SendMailCommand command, CancellationToken cancellationToken)
    {
        var mail = await _mails.FirstAsync(mil => mil.Id == command.MailId, cancellationToken);
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

        if (mail.EventId != null)
        {
            _readModelUpdater.TriggerUpdate<DuePaymentsCalculator>(null, mail.EventId);
        }
    }
}