using System;
using System.Collections.Generic;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class RegistrationMatch
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<ResponseMatch> Responses { get; set; }
        public decimal Price { get; set; }
    }
}