using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventRegistrator.Functions.Mailing
{
    public class TemplateFiller
    {
        private readonly string _template;
        private readonly IDictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly Regex _regex = new Regex(@"(?<start>\{{)+(?<property>[\w\.\[\]]+)(?<format>:[^}}]+)?(?<end>\})+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public TemplateFiller(string template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));

            Parameters = new ReadOnlyDictionary<string, string>(_parameters);

            foreach (Match match in _regex.Matches(template))
            {
                var key = match.Groups["property"].Value.ToUpper();
                if (!_parameters.ContainsKey(key))
                {
                    _parameters.Add(key, string.Empty);
                }
            }
            Prefixes = _parameters.Keys
                                  .Select(key => key?.Split('.'))
                                  .Where(parts => parts?.Length > 1)
                                  .Select(parts => parts.First()).ToList();
        }

        public string this[string key]
        {
            get => _parameters[key.ToUpper()];
            set => _parameters[key.ToUpper()] = value;
        }

        public IReadOnlyDictionary<string, string> Parameters { get; }
        public IReadOnlyList<string> Prefixes { get; set; }

        public string Fill()
        {
            return _regex.Replace(_template, GetValue);
        }

        private string GetValue(Match match)
        {
            var key = match.Groups["property"].Value.ToUpper();
            if (_parameters.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
    }
}