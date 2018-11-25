using System;
using System.IO;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Files
{
    public class SavePaymentFileCommand : IRequest, IEventBoundRequest
    {
        public string ContentType { get; set; }
        public Guid EventId { get; set; }
        public string Filename { get; set; }
        public MemoryStream FileStream { get; set; }
    }
}