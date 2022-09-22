using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;

using MediatR;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class NotReceivedMail
{
    public DateTimeOffset Created { get; set; }
    public string Events { get; set; }
    public Guid Id { get; set; }
    public string Recipients { get; set; }
    public Guid RegistrationId { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public string State { get; set; }
    public string Subject { get; set; }
}

public class NotReceivedMailsQuery : IRequest<IEnumerable<NotReceivedMail>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class NotReceivedMailsQueryHandler : IRequestHandler<NotReceivedMailsQuery, IEnumerable<NotReceivedMail>>
{
    private readonly IQueryable<MailToRegistration> _mails;

    public NotReceivedMailsQueryHandler(IQueryable<MailToRegistration> mails)
    {
        _mails = mails;
    }

    public async Task<IEnumerable<NotReceivedMail>> Handle(NotReceivedMailsQuery query,
                                                           CancellationToken cancellationToken)
    {
        var receivedStates = new[] { MailState.Open, MailState.Click };
        return (await _mails.Where(ml => ml.Registration.EventId == query.EventId
                                      && !ml.Mail.Discarded
                                      && ml.Registration.State != RegistrationState.Cancelled)
                            .Include(ml => ml.Mail)
                            .ThenInclude(ml => ml.Events)
                            .Include(ml => ml.Registration)
                            .OrderBy(ml => ml.Mail.Sent)
                            .Take(300)
                            .ToListAsync(cancellationToken))
               .Where(ml => !ml.Mail.Events.Any(mev =>
                                                    string.Equals(mev.EMail, ml.Registration.RespondentEmail,
                                                                  StringComparison.InvariantCultureIgnoreCase)
                                                 && receivedStates.Contains(mev.State)))
               .Select(ml => new NotReceivedMail
                             {
                                 Id = ml.Id,
                                 RegistrationId = ml.RegistrationId,
                                 State = ml.State.ToString(),
                                 Created = ml.Mail.Created,
                                 Recipients = ml.Registration.RespondentEmail,
                                 Sent = ml.Mail.Sent,
                                 Subject = ml.Mail.Subject,
                                 Events = ml.Mail.Events.Where(mev => string.Equals(mev.EMail,
                                                                                    ml.Registration.RespondentEmail,
                                                                                    StringComparison.InvariantCultureIgnoreCase))
                                            .Select(evt => evt.State.ToString())
                                            .Distinct()
                                            .StringJoin()
                             })
               .ToList();
    }
}