using System.IO;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Files
{
    public class SavePaymentFileCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public MemoryStream FileStream { get; set; }
    }
}