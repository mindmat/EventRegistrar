using EventRegistrar.Backend.Mailing.Send;

namespace EventRegistrar.Backend.Mailing;

public class MailViewQuery : IRequest<MailView>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailId { get; set; }
}

public class MailView
{
    public Guid Id { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public string? Recipients { get; set; }
    public EmailAddress From { get; set; } = null!;
    public DateTimeOffset Created { get; set; }
}

public class MailViewQueryHandler : IRequestHandler<MailViewQuery, MailView>
{
    private readonly IQueryable<Mail> _mails;

    public MailViewQueryHandler(IQueryable<Mail> mails)
    {
        _mails = mails;
    }

    public Task<MailView> Handle(MailViewQuery query, CancellationToken cancellationToken)
    {
        return _mails.Where(mail => mail.EventId == query.EventId && mail.Id == query.MailId)
                     .Select(mail => new MailView
                                     {
                                         Id = mail.Id,
                                         From = new EmailAddress
                                                {
                                                    Email = mail.SenderMail ?? "??",
                                                    Name = mail.SenderName ?? "??"
                                                },
                                         Recipients = mail.Recipients,
                                         Subject = mail.Subject,
                                         Content = mail.ContentHtml,
                                         Created = mail.Created
                                     })
                     .FirstAsync(cancellationToken);
    }
}