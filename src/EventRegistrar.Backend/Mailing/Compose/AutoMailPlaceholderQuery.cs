using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Compose;

public class AutoMailPlaceholderQuery : IRequest<IEnumerable<PlaceholderDescription>>
{
    public MailType MailType { get; set; }
}

public struct PlaceholderDescription
{
    public string Key { get; set; }
    public string Placeholder { get; set; }
    public string Description { get; set; }
}

public class PartnerPlaceholderAttribute : Attribute { }

public enum MailPlaceholder
{
    [PartnerPlaceholder]
    FirstName = 1,

    [PartnerPlaceholder]
    LastName = 2,

    [PartnerPlaceholder]
    Phone = 3,

    [PartnerPlaceholder]
    SpotList = 4,

    [PartnerPlaceholder]
    PartnerName = 5,

    [PartnerPlaceholder]
    Price = 6,

    [PartnerPlaceholder]
    PaidAmount = 7,

    [PartnerPlaceholder]
    DueAmount = 8,

    [PartnerPlaceholder]
    OverpaidAmount = 9,

    [PartnerPlaceholder]
    UnpaidAmount = 10,
    CancellationReason = 11,
    AcceptedDate = 12,
    Reminder1Date = 13,

    [PartnerPlaceholder]
    Comments = 14
}

public class AutoMailPlaceholderQueryHandler : IRequestHandler<AutoMailPlaceholderQuery, IEnumerable<PlaceholderDescription>>
{
    private readonly EnumTranslator _enumTranslator;

    public AutoMailPlaceholderQueryHandler(EnumTranslator enumTranslator)
    {
        _enumTranslator = enumTranslator;
    }

    public Task<IEnumerable<PlaceholderDescription>> Handle(AutoMailPlaceholderQuery query, CancellationToken cancellationToken)
    {
        var values = Enum.GetValues<MailPlaceholder>()
                         .SelectMany(mph => GetPossiblePlaceholders(query.MailType, mph));
        return Task.FromResult(values);
    }

    private IEnumerable<PlaceholderDescription> GetPossiblePlaceholders(MailType mailType, MailPlaceholder placeholder)
    {
        yield return new PlaceholderDescription
                     {
                         Key = placeholder.ToString(),
                         Placeholder = $"{{{{{placeholder}}}}}",
                         Description = _enumTranslator.Translate(placeholder)
                     };

        if (mailType.HasAttribute<PartnerMailTypeAttribute>()
         && placeholder.HasAttribute<PartnerPlaceholderAttribute>())
        {
            var keyLeader = $"{Role.Leader}.{placeholder}";
            yield return new PlaceholderDescription
                         {
                             Key = keyLeader,
                             Placeholder = $"{{{{{keyLeader}}}}}",
                             Description = $"{_enumTranslator.Translate(Role.Leader)}: {_enumTranslator.Translate(placeholder)}"
                         };
            var keyFollower = $"{Role.Follower}.{placeholder}";
            yield return new PlaceholderDescription
                         {
                             Key = keyFollower,
                             Placeholder = $"{{{{{keyFollower}}}}}",
                             Description = $"{_enumTranslator.Translate(Role.Follower)}: {_enumTranslator.Translate(placeholder)}"
                         };
        }
    }
}