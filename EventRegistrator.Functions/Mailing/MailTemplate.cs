using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;

namespace EventRegistrator.Functions.Mailing
{
    public class MailTemplate : Entity
    {
        public Guid EventId { get; set; }
    }
}