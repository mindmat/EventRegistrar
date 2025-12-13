using System.Text;
using System.Web;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;

using HtmlAgilityPack;

namespace EventRegistrar.Backend.Mailing;

public class PendingMailsCalculator(IQueryable<Mail> _mails) : ReadModelCalculator<IEnumerable<PendingMailListItem>>
{
    public override string QueryName => nameof(PendingMailsQuery);
    public override bool IsDateDependent => false;
    private const int _startLength = 200;

    protected override async Task<(IEnumerable<PendingMailListItem> ReadModel, MenuNodeCalculation? MenuNode)>
        CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var mails = await _mails.Where(mail => mail.EventId == eventId
                                            && mail.Withhold
                                            && !mail.Discarded)
                                .OrderByDescending(mail => mail.Created)
                                .Select(mail => new PendingMailListItem
                                                {
                                                    Id = mail.Id,
                                                    RecipientsEmails = mail.Recipients,
                                                    RecipientsNames = mail.Registrations!.Select(reg => $"{reg.Registration!.RespondentFirstName} {reg.Registration.RespondentLastName}")
                                                                          .StringJoin(", "),
                                                    Subject = mail.Subject,
                                                    Created = mail.Created,
                                                    Type = mail.Type,
                                                    ContentStart = GetContentStart(mail.ContentHtml)
                                                })
                                .ToListAsync(cancellationToken);

        var node = new MenuNodeCalculation
                   {
                       Key = MenuNodeKey.PendingMails
                   };

        if (mails.Count > 0)
        {
            node.Content = $"{mails.Count}";
            node.Style = MenuNodeStyle.ToDo;
        }

        return (mails, node);
    }

    private static string? GetContentStart(string? mailContentHtml)
    {
        if (mailContentHtml == null)
        {
            return null;
        }

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(mailContentHtml);
            var stringBuilder = new StringBuilder(500);
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
        catch
        {
            return null;
        }
    }
}

public class PendingMailListItem
{
    public Guid Id { get; set; }
    public string? RecipientsEmails { get; set; }
    public string? RecipientsNames { get; set; }
    public string? Subject { get; set; }
    public string? ContentStart { get; set; }
    public DateTimeOffset Created { get; set; }
    public MailType? Type { get; set; }
}