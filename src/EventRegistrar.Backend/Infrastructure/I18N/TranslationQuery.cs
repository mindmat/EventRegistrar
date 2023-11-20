using System.Collections;
using System.Globalization;
using System.Resources;

using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Infrastructure.I18N;

public class TranslationQuery : IRequest<IDictionary<string, string>>
{
    public string? Language { get; set; }
}

public class TranslationQueryHandler : IRequestHandler<TranslationQuery, IDictionary<string, string>>
{
    public Task<IDictionary<string, string>> Handle(TranslationQuery query, CancellationToken cancellationToken)
    {
        var culture = query.Language == null
                          ? CultureInfo.InvariantCulture
                          : new CultureInfo(query.Language);
        IDictionary<string, string> dict = GetTranslations(Resources.ResourceManager, culture)
            .ToDictionary(entry => entry.Key,
                          entry => entry.Value);
        return Task.FromResult(dict);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetTranslations(ResourceManager resourceManager, CultureInfo culture)
    {
        var keys = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, true)
                                  ?.OfType<DictionaryEntry>()
                                  .ToDictionary(entry => entry.Key, entry => entry.Value)
                                  .Select(entry => entry.Key)
                                  .OfType<string>()
                ?? Enumerable.Empty<string>();

        return keys.Select(key => new KeyValuePair<string, string>(key, resourceManager.GetString(key, culture) ?? key));
    }
}