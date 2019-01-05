using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MailKit;
using MailKit.Net.Imap;
using MediatR;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ImportMailsFromImapCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class ImportMailsFromImapCommandHandler : IRequestHandler<ImportMailsFromImapCommand>
    {
        private readonly ExternalMailConfiguration _configuration;
        private readonly IEventBus _eventBus;
        private readonly IRepository<ImportedMail> _importedMails;
        private readonly ILogger _log;

        public ImportMailsFromImapCommandHandler(ExternalMailConfiguration configuration,
                                                 IRepository<ImportedMail> importedMails,
                                                 IEventBus eventBus,
                                                 ILogger log)
        {
            _configuration = configuration;
            _importedMails = importedMails;
            _eventBus = eventBus;
            _log = log;
        }

        public async Task<Unit> Handle(ImportMailsFromImapCommand command, CancellationToken cancellationToken)
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(_configuration.ImapHost, _configuration.ImapPort, true, cancellationToken);

                await client.AuthenticateAsync(_configuration.Username, _configuration.Password, cancellationToken);

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
                    {
                        // mail has been imported earlier
                        continue;
                    }

                    var mail = new ImportedMail
                    {
                        Id = Guid.NewGuid(),
                        EventId = command.EventId,
                        ContentHtml = message.HtmlBody,
                        ContentPlainText = message.TextBody,
                        Imported = DateTime.UtcNow,
                        MessageIdentifier = message.MessageId,
                        Recipients = message.To.OfType<MailboxAddress>()?.Select(rcp => rcp.Address).StringJoin(";"),
                        SenderMail = message.From.OfType<MailboxAddress>().FirstOrDefault()?.Address,
                        SenderName = message.From.FirstOrDefault()?.Name,
                        Subject = message.Subject,
                        Date = message.Date.ToUniversalTime().DateTime
                    };

                    mail.SendGridMessageId = message.References.FirstOrDefault(rfr => rfr.EndsWith("sendgrid.net"));

                    await _importedMails.InsertOrUpdateEntity(mail, cancellationToken);

                    _eventBus.Publish(new ExternalMailImported
                    {
                        ImportedMailId = mail.Id,
                        ExternalDate = mail.Date
                    });
                }

                await client.DisconnectAsync(true, cancellationToken);
            }

            return Unit.Value;
        }
    }
}