using System.Collections;
using System.Globalization;
using System.Resources;

using EventRegistrar.Backend.Properties;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure.I18N;

public class TranslationQuery : IRequest<IDictionary<string, string>>
{
    public string? Language { get; set; }
}

public class TranslationQueryHandler : RequestHandler<TranslationQuery, IDictionary<string, string>>
{
    protected override IDictionary<string, string> Handle(TranslationQuery query)
    {
        var dict = GetTranslations(Resources.ResourceManager, CultureInfo.CurrentCulture)
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        return dict;
    }

    private static IEnumerable<KeyValuePair<string, string>> GetTranslations(ResourceManager resourceManager, CultureInfo culture)
    {
        var keys = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
                                  ?.OfType<DictionaryEntry>()
                                  .Select(entry => entry.Key)
                                  .OfType<string>()
                ?? Enumerable.Empty<string>();

        return keys.Select(key => new KeyValuePair<string, string>(key, resourceManager.GetString(key, culture) ?? key));
    }
}