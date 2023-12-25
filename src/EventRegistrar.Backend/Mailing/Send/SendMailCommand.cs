using System.Net;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Mailing.InvalidAddresses;
using EventRegistrar.Backend.Payments.Due;

using MailKit.Net.Smtp;

using MimeKit;

using PostmarkDotNet;
using PostmarkDotNet.Model;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrar.Backend.Mailing.Send;

public class SendMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailId { get; set; }
}

public class SendMailCommandHandler(ILogger logger,
                                    IRepository<Mail> mails,
                                    ChangeTrigger changeTrigger,
                                    MailConfiguration mailConfiguration,
                                    SecretReader secretReader,
                                    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<SendMailCommand>
{
    private const string MessageIdHeader = "X-Message-Id";

    public async Task Handle(SendMailCommand command, CancellationToken cancellationToken)
    {
        var mail = await mails.AsTracking()
                              .Include(mail => mail.Registrations!)
                              .ThenInclude(map => map.Registration)
                              .FirstAsync(mil => mil.Id == command.MailId, cancellationToken);

        var recipients = mail.Registrations!
                             .Where(reg => (reg.Email ?? reg.Registration!.RespondentEmail) != null)
                             .Select(reg => new EmailAddress
                                            {
                                                Email = (reg.Email ?? reg.Registration!.RespondentEmail)!,
                                                Name = reg.Registration!.RespondentFirstName ?? reg.Registration.RespondentLastName
                                            })
                             // group by email address (leader and follower could use the same)
                             .GroupBy(adr => adr.Email)
                             .Select(grp => new EmailAddress
                                            {
                                                Email = grp.Key,
                                                Name = grp.Select(reg => reg.Name)
                                                          .StringJoinNullable(" & ") // avoid ',' obviously SendGrid interprets commas
                                            });

        var sender = new EmailAddress
                     {
                         Email = mail.SenderMail ?? mailConfiguration.SenderMail,
                         Name = mail.SenderName ?? mailConfiguration.SenderName
                     };

        if (mailConfiguration.MailSender == MailSender.Smtp)
        {
            var config = mailConfiguration.SmtpConfiguration;
            if (config is { Host: not null, Port: not null, Username: not null, Password: not null }
             && mail.SenderMail != null)
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(sender.Name, sender.Email));
                foreach (var recipient in recipients)
                {
                    mailMessage.To.Add(new MailboxAddress(recipient.Name, recipient.Email));
                }

                mailMessage.Subject = mail.Subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = mail.ContentHtml };
                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.MessageId = mail.Id.ToString();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(config.Host, config.Port.Value, true, cancellationToken);
                await smtpClient.AuthenticateAsync(config.Username, config.Password, cancellationToken);
                await smtpClient.SendAsync(mailMessage, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);

                mail.Sent ??= dateTimeProvider.Now;
                mail.SentBy = MailSender.Smtp;
            }
        }
        else if (mailConfiguration.MailSender == MailSender.SendGrid)
        {
            var msg = new SendGridMessage
                      {
                          From = new SendGrid.Helpers.Mail.EmailAddress(sender.Email, sender.Name),
                          Subject = mail.Subject,
                          PlainTextContent = mail.ContentPlainText,
                          HtmlContent = mail.ContentHtml,
                          CustomArgs = new Dictionary<string, string> { { nameof(SendGridEvent.MailId), mail.Id.ToString() } }
                      };

            msg.AddTos(recipients.Select(to => new SendGrid.Helpers.Mail.EmailAddress(to.Email, to.Name))
                                 .ToList());

            // send mail
            var apiKey = await secretReader.GetSendGridApiKey(cancellationToken);
            var client = new SendGridClient(apiKey);

            var response = await client.SendEmailAsync(msg, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
            {
                logger.LogWarning($"Send mail with SendGrid failed: status {response.StatusCode}, Body {await response.Body.ReadAsStringAsync(cancellationToken)}");
            }
            else
            {
                if (response.Headers.TryGetValues(MessageIdHeader, out var values))
                {
                    mail.Sent ??= dateTimeProvider.Now;
                    mail.SentBy = MailSender.SendGrid;
                    mail.MailSenderMessageId = values.FirstOrDefault();
                }
                else
                {
                    logger.LogWarning($"Header {MessageIdHeader} not present in response, cannot determine Message-ID");
                }
            }
        }
        else if (mailConfiguration.MailSender == MailSender.Postmark)
        {
            var postmarkToken = await secretReader.GetPostmarkToken(cancellationToken);
            var message = new PostmarkMessage
                          {
                              To = recipients.Select(to => to.ToNameMail()).StringJoin(","),
                              From = sender.ToNameMail(),
                              TrackOpens = true,
                              Subject = mail.Subject,
                              TextBody = mail.ContentPlainText,
                              HtmlBody = mail.ContentHtml,
                              Headers = new HeaderCollection { new(nameof(SendGridEvent.MailId), mail.Id.ToString()) }
                          };

            var client = new PostmarkClient(postmarkToken);
            var sendResult = await client.SendMessageAsync(message);
            if (sendResult.Status != PostmarkStatus.Success)
            {
                logger.LogWarning($"Send mail with postmark failed: status {sendResult.Status}, Message {sendResult.Message}");
            }
            else
            {
                mail.Sent ??= dateTimeProvider.Now;
                mail.SentBy = MailSender.Postmark;
                mail.MailSenderMessageId = sendResult.MessageID.ToString();
            }
        }

        if (mail.EventId != null)
        {
            changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, mail.EventId);
            changeTrigger.QueryChanged<MailDeliverySuccessQuery>(mail.EventId.Value);
        }
    }
}