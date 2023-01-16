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

public class ImportMailsFromImapCommandHandler : AsyncRequestHandler<ImportMailsFromImapCommand>
{
    private readonly ExternalMailConfigurations _configurations;
    private readonly IEventBus _eventBus;
    private readonly IRepository<ImportedMail> _importedMails;
    private readonly ILogger _log;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ImportMailsFromImapCommandHandler(ExternalMailConfigurations configurations,
                                             IRepository<ImportedMail> importedMails,
                                             IEventBus eventBus,
                                             ILogger log,
                                             IDateTimeProvider dateTimeProvider)
    {
        _configurations = configurations;
        _importedMails = importedMails;
        _eventBus = eventBus;
        _log = log;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task Handle(ImportMailsFromImapCommand command, CancellationToken cancellationToken)
    {
        if (_configurations.MailConfigurations == null)
        {
            return;
        }

        var minDate = _dateTimeProvider.Now.AddMonths(-2);
        foreach (var mailConfiguration in _configurations.MailConfigurations)
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

            _log.LogInformation("Total messages: {0}", inbox.Count);
            _log.LogInformation("Recent messages: {0}", inbox.Recent);

            for (var i = 0; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i, cancellationToken);
                if (message.Date < minDate
                 && _importedMails.Any(iml => iml.EventId == command.EventId
                                           && iml.MessageIdentifier == message.MessageId))
                    // mail has been imported earlier
                {
                    continue;
                }

                var mail = new ImportedMail
                           {
                               Id = Guid.NewGuid(),
                               EventId = command.EventId,
                               ContentHtml = message.HtmlBody,
                               ContentPlainText = message.TextBody,
                               Imported = _dateTimeProvider.Now,
                               MessageIdentifier = message.MessageId,
                               Recipients = message.To.OfType<MailboxAddress>()
                                                   .Select(rcp => rcp.Address)
                                                   .StringJoin(";"),
                               SenderMail = message.From.OfType<MailboxAddress>().FirstOrDefault()?.Address,
                               SenderName = message.From.FirstOrDefault()?.Name,
                               Subject = message.Subject,
                               Date = message.Date,
                               SendGridMessageId =
                                   message.References.FirstOrDefault(rfr => rfr.EndsWith("sendgrid.net"))
                           };

                _importedMails.InsertObjectTree(mail);

                _eventBus.Publish(new ExternalMailImported
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