using System;
using System.Collections.Generic;

namespace EventRegistrator.Functions.GoogleForms
{
    public class Registration
    {
        public string Email { get; set; }
        public IEnumerable<ResponseData> Responses { get; set; }
        public DateTime Timestamp { get; set; }
    }
}