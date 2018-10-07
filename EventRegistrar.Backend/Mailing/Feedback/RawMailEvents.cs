using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class RawMailEvents : Entity
    {
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public bool Processed { get; set; }
    }
}