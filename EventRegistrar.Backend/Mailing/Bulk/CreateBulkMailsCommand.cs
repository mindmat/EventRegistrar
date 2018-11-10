using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class CreateBulkMailsCommand : IRequest, IEventBoundRequest
    {
        public string BulkMailKey { get; set; }
        public Guid EventId { get; set; }
    }
}