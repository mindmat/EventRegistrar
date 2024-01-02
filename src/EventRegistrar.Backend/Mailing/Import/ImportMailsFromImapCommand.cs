using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MailKit;
using MailKit.Net.Imap;

using MimeKit;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportMailsFromImapCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ImportMailsFromImapCommandHandler(ExternalMailConfigurations configurations,
                                               IRepository<ImportedMail> importedMails,
                                               IEventBus eventBus,
                                               ILogger log,
                                               IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ImportMailsFromImapCommand>
{
    public async Task Handle(ImportMailsFromImapCommand command, CancellationToken cancellationToken)
    {
        if (configurations.MailConfigurations == null)
        {
            return;
        }

        foreach (var mailConfiguration in configurations.MailConfigurations)
        {
            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (sender,
                                                          certificate,
                                                          chain,
                                                          errors) => true;
            await client.ConnectAsync(mailConfiguration.ImapHost, mailConfiguration.ImapPort, true, cancellationToken);
            await client.AuthenticateAsync(mailConfiguration.Username, mailConfiguration.Password, cancellationToken);

            // The Inbox folder is always available on all IMAP servers...
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            log.LogInformation("Total messages: {0}", inbox.Count);
            log.LogInformation("Recent messages: {0}", inbox.Recent);
            var minDate = mailConfiguration.ImportMailsSince
                       ?? dateTimeProvider.Now.AddMonths(-2);

            for (var i = 0; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i, cancellationToken);
                if (message.Date < minDate
                 && await importedMails.AnyAsync(iml => iml.EventId == command.EventId
                                                     && iml.MessageIdentifier == message.MessageId, cancellationToken))
                    // mail has been imported earlier
                {
                    continue;
                }

                var mail = new ImportedMail
                           {
                               Id = Guid.NewGuid(),
                               ExternalMailConfigurationId = mailConfiguration.Id,
                               EventId = command.EventId,
                               ContentHtml = message.HtmlBody,
                               ContentPlainText = message.TextBody,
                               Imported = dateTimeProvider.Now,
                               MessageIdentifier = message.MessageId,
                               Recipients = message.To.OfType<MailboxAddress>()
                                                   .Select(rcp => rcp.Address)
                                                   .StringJoin(";"),
                               SenderMail = message.From.OfType<MailboxAddress>().FirstOrDefault()?.Address,
                               SenderName = message.From.FirstOrDefault()?.Name,
                               Subject = message.Subject,
                               Date = message.Date,
                               SendGridMessageId = message.References.FirstOrDefault(rfr => rfr.EndsWith("sendgrid.net"))
                           };

                importedMails.InsertObjectTree(mail);

                eventBus.Publish(new ExternalMailImported
                                 {
                                     ImportedMailId = mail.Id,
                                     ExternalDate = mail.Date,
                                     Subject = message.Subject,
                                     From = message.From.OfType<MailboxAddress>()
                                                   .Select(rcp => rcp.Address)
                                                   .StringJoin(";")
                                 });
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}