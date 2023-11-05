using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Mailing;

public class ReleaseMailsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<Guid> MailIds { get; set; } = null!;
}

public class ReleaseMailsCommandHandler : AsyncRequestHandler<ReleaseMailsCommand>
{
    private readonly IRepository<Mail> _mails;
    private readonly CommandQueue _commandQueue;
    private readonly IEventBus _eventBus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ChangeTrigger _changeTrigger;

    public ReleaseMailsCommandHandler(IRepository<Mail> mails,
                                      CommandQueue commandQueue,
                                      IEventBus eventBus,
                                      IDateTimeProvider dateTimeProvider,
                                      ChangeTrigger changeTrigger)
    {
        _mails = mails;
        _commandQueue = commandQueue;
        _eventBus = eventBus;
        _dateTimeProvider = dateTimeProvider;
        _changeTrigger = changeTrigger;
    }

    protected override async Task Handle(ReleaseMailsCommand command, CancellationToken cancellationToken)
    {
        var withheldMails = await _mails.Where(mail => command.MailIds.Contains(mail.Id)
                                                    && !mail.Discarded)
                                        .Include(mail => mail.Registrations!)
                                        .ThenInclude(map => map.Registration)
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
                                      To = withheldMail.Registrations!
                                                       .GroupBy(reg => reg.Registration!.RespondentEmail?.ToLowerInvariant())
                                                       .Select(grp => new EmailAddress
                                                                      {
                                                                          Email = grp.Key,
                                                                          Name = grp.Select(reg => reg.Registration!.RespondentFirstName)
                                                                                    .StringJoin(" & ") // avoid ',' obviously SendGrid interprets commas
                                                                      })
                                                       .ToList()
                                  };

            withheldMail.Withhold = false;
            withheldMail.Sent = _dateTimeProvider.Now;

            _commandQueue.EnqueueCommand(sendMailCommand);

            _eventBus.Publish(new MailReleased
                              {
                                  MailId = withheldMail.Id,
                                  To = sendMailCommand.To.Select(to => $"{to.Name} - {to.Email}").StringJoin(),
                                  Subject = sendMailCommand.Subject
                              });

            withheldMail.Registrations!.ForEach(reg => _changeTrigger.TriggerUpdate<RegistrationCalculator>(reg.RegistrationId, command.EventId));
        }

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(PendingMailsQuery)
                          });
    }
}