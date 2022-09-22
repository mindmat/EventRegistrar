using System.Text;
using System.Web;

using HtmlAgilityPack;

namespace EventRegistrar.Backend.Mailing;

public class PendingMailsQuery : IRequest<IEnumerable<PendingMailListItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PendingMailListItem
{
    public Guid Id { get; set; }
    public string? Recipients { get; set; }
    public string? Subject { get; set; }
    public string? ContentStart { get; set; }
    public DateTimeOffset Created { get; set; }
}

public class PendingMailsQueryHandler : IRequestHandler<PendingMailsQuery, IEnumerable<PendingMailListItem>>
{
    private readonly IQueryable<Mail> _mails;
    private const int _startLength = 200;

    public PendingMailsQueryHandler(IQueryable<Mail> mails)
    {
        _mails = mails;
    }

    public async Task<IEnumerable<PendingMailListItem>> Handle(PendingMailsQuery query, CancellationToken cancellationToken)
    {
        var mails = await _mails.Where(mail => mail.EventId == query.EventId
                                            && mail.Withhold
                                            && !mail.Discarded)
                                .OrderByDescending(mail => mail.Created)
                                .Select(mail => new PendingMailListItem
                                                {
                                                    Id = mail.Id,
                                                    Recipients = mail.Recipients,
                                                    Subject = mail.Subject,
                                                    Created = mail.Created,
                                                    ContentStart = GetContentStart(mail.ContentHtml)
                                                })
                                .ToListAsync(cancellationToken);
        return mails;
    }

    private static string? GetContentStart(string? mailContentHtml)
    {
        if (mailContentHtml == null)
        {
            return null;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(mailContentHtml);
        var stringBuilder = new StringBuilder();
        foreach (var node in doc.DocumentNode.SelectNodes("//text()"))
        {
            if (stringBuilder.Length > 200)
            {
                return stringBuilder.ToString()[.._startLength] + "...";
            }

            stringBuilder.Append(HttpUtility.HtmlDecode(node.InnerText) + " ");
        }

        return stringBuilder.ToString();
    }
}