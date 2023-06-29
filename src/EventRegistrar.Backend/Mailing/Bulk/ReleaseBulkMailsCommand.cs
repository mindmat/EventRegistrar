using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class ReleaseBulkMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string BulkMailKey { get; set; } = null!;
}

public class ReleaseBulkMailsCommandHandler : AsyncRequestHandler<ReleaseBulkMailsCommand>
{
    private const int ChunkSize = 100;
    private readonly IRepository<Mail> _mails;
    private readonly CommandQueue _commandQueue;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReleaseBulkMailsCommandHandler(IRepository<Mail> mails,
                                          CommandQueue commandQueue,
                                          IEventBus eventBus,
                                          IDateTimeProvider dateTimeProvider)
    {
        _mails = mails;
        _commandQueue = commandQueue;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task Handle(ReleaseBulkMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await _mails.Where(mail => mail.BulkMailKey == command.BulkMailKey
                                                    && mail.EventId == command.EventId
                                                    && mail.Withhold
                                                    && !mail.Discarded)
                                        .Include(mail => mail.Registrations!)
                                        .ThenInclude(map => map.Registration)
                                        .Take(ChunkSize)
                                        .ToListAsync(cancellationToken);

        foreach (var withheldMail in withheldMails)
        {
            var sendMailCommand = new SendMailCommand
                                  {
                                      EventId = withheldMail.EventId!.Value,
                                      MailId = withheldMail.Id,
                                      ContentHtml = withheldMail.ContentHtml,
                                      ContentPlainText = withheldMail.ContentPlainText,
                                      Subject = withheldMail.Subject,
                                      Sender = new EmailAddress
                                               {
                                                   Email = withheldMail.SenderMail,
                                                   Name = withheldMail.SenderName
                                               },
                                      To = withheldMail.Registrations!.Select(reg => new EmailAddress
                                                                                     {
                                                                                         Email = reg.Registration!.RespondentEmail,
                                                                                         Name = reg.Registration!.RespondentFirstName
                                                                                     })
                                                       .ToList()
                                  };

            withheldMail.Withhold = false;
            withheldMail.Sent = _dateTimeProvider.Now;

            _commandQueue.EnqueueCommand(sendMailCommand);
        }

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(GeneratedBulkMailsQuery)
                          });
        if (withheldMails.Count >= ChunkSize)
        {
            // enqueue next chunk
            _commandQueue.EnqueueCommand(new ReleaseBulkMailsCommand
                                         {
                                             EventId = command.EventId,
                                             BulkMailKey = command.BulkMailKey
                                         });
        }
    }
}