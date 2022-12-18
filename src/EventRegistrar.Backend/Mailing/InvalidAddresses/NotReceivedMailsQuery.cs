using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class NotReceivedMailsQuery : IRequest<IEnumerable<ProblematicEmail>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? SearchString { get; set; }
}

public class NotReceivedMailsQueryHandler : IRequestHandler<NotReceivedMailsQuery, IEnumerable<ProblematicEmail>>
{
    private readonly IQueryable<MailToRegistration> _mails;
    private readonly EnumTranslator _enumTranslator;

    public NotReceivedMailsQueryHandler(IQueryable<MailToRegistration> mails,
                                        EnumTranslator enumTranslator)
    {
        _mails = mails;
        _enumTranslator = enumTranslator;
    }

    public async Task<IEnumerable<ProblematicEmail>> Handle(NotReceivedMailsQuery query,
                                                            CancellationToken cancellationToken)
    {
        const int maxCount = 300;
        var receivedStates = new MailState?[] { MailState.Delivered, MailState.Open, MailState.Click };
        return await _mails.Where(ml => ml.Registration!.EventId == query.EventId
                                     && !ml.Mail!.Discarded
                                     && !ml.Mail!.Withhold
                                     && ml.Registration.State != RegistrationState.Cancelled
                                     && !receivedStates.Contains(ml.State)
                                     && ml.Email != null)
                           .WhereIf(query.SearchString != null, ml => ml.Email!.Contains(query.SearchString!))
                           .GroupBy(ml => ml.Email)
                           .Select(grp => new ProblematicEmail
                                          {
                                              RegistrationId = grp.First().RegistrationId,
                                              Email = grp.Key!,
                                              NotReceivedMails = grp.OrderByDescending(mtr => mtr.Mail!.Created)
                                                                    .Select(mtr => new NotReceivedMail
                                                                                   {
                                                                                       MailId = mtr.MailId,
                                                                                       RegistrationId = mtr.RegistrationId,
                                                                                       State = _enumTranslator.Translate(mtr.State),
                                                                                       Created = mtr.Mail!.Created,
                                                                                       Recipients = mtr.Registration!.RespondentEmail,
                                                                                       Sent = mtr.Mail.Sent,
                                                                                       Subject = mtr.Mail.Subject
                                                                                   })
                                          })
                           .OrderBy(pml => pml.Email)
                           .Take(maxCount + 1)
                           .ToListAsync(cancellationToken);
    }
}

public class ProblematicEmail
{
    public string Email { get; set; } = null!;
    public Guid RegistrationId { get; set; }
    public IEnumerable<NotReceivedMail> NotReceivedMails { get; set; } = null!;
}

public class NotReceivedMail
{
    public Guid MailId { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public string? Recipients { get; set; }
    public Guid RegistrationId { get; set; }
    public string? State { get; set; }
    public string? Subject { get; set; }
}