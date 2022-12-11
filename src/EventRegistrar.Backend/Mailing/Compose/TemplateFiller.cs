using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace EventRegistrar.Backend.Mailing.Compose;

public class TemplateFiller
{
    private readonly IDictionary<string, string?> _parameters = new Dictionary<string, string?>();

    private readonly Regex _regex = new(@"(?<start>\{{)+(?<property>[\w\.\[\]]+)(?<format>:[^}}]+)?(?<end>\})+",
                                        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly string _template;

    public TemplateFiller(string template)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));

        Parameters = new ReadOnlyDictionary<string, string?>(_parameters);

        foreach (Match match in _regex.Matches(template))
        {
            var key = match.Groups["property"].Value.ToUpper();
            if (!_parameters.ContainsKey(key))
            {
                _parameters.Add(key, string.Empty);
            }
        }

        Prefixes = _parameters.Keys
                              .Select(key => key.Split('.'))
                              .Where(parts => parts.Length > 1)
                              .Select(parts => parts.First())
                              .Distinct()
                              .ToList();
    }

    public IReadOnlyDictionary<string, string?> Parameters { get; }

    public IReadOnlyList<string> Prefixes { get; }

    public string? this[string key]
    {
        get => _parameters[key.ToUpper()];
        set => _parameters[key.ToUpper()] = value;
    }

    public string Fill()
    {
        return _regex.Replace(_template, GetValue);
    }

    private string GetValue(Match match)
    {
        var key = match.Groups["property"].Value.ToUpper();
        return _parameters.TryGetValue(key, out var value) && value != null
                   ? value
                   : string.Empty;
    }
}