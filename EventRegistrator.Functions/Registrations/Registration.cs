using System;
using System.Collections.Generic;
using EventRegistrator.Functions.GoogleForms;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Registrations
{
    public class Registration : Entity
    {
        public Guid RegistrationFormId { get; set; }
        public string ExternalIdentifier { get; set; }
        public string RespondentEmail { get; set; }
        public IEnumerable<ResponseData> Responses { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}