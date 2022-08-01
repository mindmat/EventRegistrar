using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;

using MediatR;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class ReleaseBulkMailsCommand : IRequest, IEventBoundRequest
{
    public string BulkMailKey { get; set; } = null!;
    public Guid EventId { get; set; }
}

public class ReleaseBulkMailsCommandHandler : IRequestHandler<ReleaseBulkMailsCommand>
{
    private readonly IRepository<Mail> _mails;
    private readonly CommandQueue _commandQueue;

    public ReleaseBulkMailsCommandHandler(IRepository<Mail> mails,
                                          CommandQueue commandQueue)
    {
        _mails = mails;
        _commandQueue = commandQueue;
    }

    public async Task<Unit> Handle(ReleaseBulkMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await _mails.Where(mail => mail.MailTemplate!.BulkMailKey == command.BulkMailKey
                                                    && mail.EventId == command.EventId
                                                    && mail.Withhold
                                                    && !mail.Discarded)
                                        .Include(mail => mail.Registrations!)
                                        .ThenInclude(map => map.Registration)
                                        .Take(100)
                                        .ToListAsync(cancellationToken);

        foreach (var withheldMail in withheldMails)
        {
            var sendMailCommand = new SendMailCommand
                                  {
                                      MailId = withheldMail.Id,
                                      ContentHtml = withheldMail.ContentHtml,
                                      ContentPlainText = withheldMail.ContentPlainText,
                                      Subject = withheldMail.Subject,
                                      Sender = new EmailAddress
                                               { Email = withheldMail.SenderMail, Name = withheldMail.SenderName },
                                      To = withheldMail.Registrations.Select(reg => new EmailAddress
                                                                                    {
                                                                                        Email = reg.Registration.RespondentEmail,
                                                                                        Name = reg.Registration.RespondentFirstName
                                                                                    })
                                                       .ToList()
                                  };

            withheldMail.Withhold = false;
            withheldMail.Sent = DateTime.UtcNow;

            _commandQueue.EnqueueCommand(sendMailCommand);
        }

        return Unit.Value;
    }
}