using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates;

public class MailTypesQueryHandler : IRequestHandler<MailTypesQuery, IEnumerable<MailTypeItem>>
{
    public Task<IEnumerable<MailTypeItem>> Handle(MailTypesQuery request, CancellationToken cancellationToken)
    {
        var resources = Resources.ResourceManager;
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