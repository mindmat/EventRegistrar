using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace EventRegistrator.Functions.Mailing
{
    public class TemplateFiller
    {
        private readonly string _template;
        private readonly IDictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly MatchCollection _matches;
        private readonly Regex _regex = new Regex(@"(?<start>\{{)+(?<property>[\w\.\[\]]+)(?<format>:[^}}]+)?(?<end>\})+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public TemplateFiller(string template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));

            Parameters = new ReadOnlyDictionary<string, string>(_parameters);

            _matches = _regex.Matches(template);
            foreach (Match match in _matches)
            {
                _parameters.Add(match.Groups["property"].Value, string.Empty);
            }

            //r.Matches(template).Select(m=> m);
        }

        public string this[string key]
        {
            get => _parameters[key];
            set => _parameters[key] = value;
        }

        public IReadOnlyDictionary<string, string> Parameters { get; }

        //public static string FormatWith(this string format, IFormatProvider provider, object source)
        //{
        //    List<object> values = new List<object>();
        //    string rewrittenFormat = r.Replace(format, delegate (Match m)
        //    {
        //        Group startGroup = m.Groups["start"];
        //        Group propertyGroup = m.Groups["property"];
        //        Group formatGroup = m.Groups["format"];
        //        Group endGroup = m.Groups["end"];

        //        values.Add((propertyGroup.Value == "0")
        //            ? source
        //            : DataBinder.Eval(source, propertyGroup.Value));

        //        return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
        //               + new string('}', endGroup.Captures.Count);
        //    });

        //    return string.Format(provider, rewrittenFormat, values.ToArray());
        //}
        public string Fill()
        {
            return _regex.Replace(_template, GetValue);
        }

        private string GetValue(Match match)
        {
            var key = match.Groups["property"].Value;
            //string value;
            if (_parameters.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
    }
}