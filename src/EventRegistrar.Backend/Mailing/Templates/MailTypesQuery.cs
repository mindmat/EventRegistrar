using EventRegistrar.Backend.Authorization;

namespace EventRegistrar.Backend.Mailing.Templates;

public class MailTypesQuery : IRequest<IEnumerable<MailTypeItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class MailTypesQueryHandler : IRequestHandler<MailTypesQuery, IEnumerable<MailTypeItem>>
{
    public Task<IEnumerable<MailTypeItem>> Handle(MailTypesQuery query, CancellationToken cancellationToken)
    {
        var resources = Properties.Resources.ResourceManager;
        var list = Enum.GetValues(typeof(MailType))
                       .Cast<MailType>()
                       .Select(mtp => new MailTypeItem
                                      {
                                          Type = mtp,
                                          UserText = resources.GetString($"MailType_{mtp}") ?? mtp.ToString()
                                      })
                       .OrderBy(enm => enm.UserText)
                       .AsEnumerable();
        return Task.FromResult(list);
    }
}

public class MailTypeItem
{
    public string? BulkMailKey { get; set; }
    public MailType? Type { get; set; }
    public string UserText { get; set; } = null!;
}