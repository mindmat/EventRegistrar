using System.Collections.Generic;

namespace EventRegistrator.Functions.GoogleForms
{
    public class Registration
    {
        public string ExternalIdentifier { get; set; }
        public string Email { get; set; }
        public IEnumerable<ResponseData> Responses { get; set; }
    }
}