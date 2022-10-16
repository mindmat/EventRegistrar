using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MailKit;
using MailKit.Net.Imap;

using MediatR;

using MimeKit;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportMailsFromImapCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ImportMailsFromImapCommandHandler : IRequestHandler<ImportMailsFromImapCommand>
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

    public async Task<Unit> Handle(ImportMailsFromImapCommand command, CancellationToken cancellationToken)
    {
        if (_configurations.MailConfigurations == null)
        {
            return Unit.Value;
        }

        foreach (var mailConfiguration in _configurations.MailConfigurations)
        {
            using var client = new ImapClient();
            await client.ConnectAsync(mailConfiguration.ImapHost, mailConfiguration.ImapPort, true, cancellationToken);
            await client.AuthenticateAsync(mailConfiguration.Username, mailConfiguration.Password, cancellationToken);

            // The Inbox folder is always available on all IMAP servers...
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            _log.LogInformation("Total messages: {0}", inbox.Count);
            _log.LogInformation("Recent messages: {0}", inbox.Recent);

            for (var i = 0; i < inbox.Count; i++)
            {
                var message = inbox.GetMessage(i);
                if (_importedMails.Any(iml => iml.EventId == command.EventId
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

                await _importedMails.InsertOrUpdateEntity(mail, cancellationToken);

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

        return Unit.Value;
    }
}