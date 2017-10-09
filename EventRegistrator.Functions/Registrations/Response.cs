using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;

namespace EventRegistrator.Functions.Registrations
{
    public class Response : Entity
    {
        public Guid? QuestionId { get; set; }
        public string ResponseString { get; set; }
    }
}