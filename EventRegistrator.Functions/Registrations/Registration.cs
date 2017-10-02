using System.Collections.Generic;

namespace EventRegistrator.Functions.Registrations
{
    public class Registration
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IEnumerable<ResponseData> Responses { get; set; }
    }
}