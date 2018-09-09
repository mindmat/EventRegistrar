using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class CreateBulkMailsCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public string TemplateKey { get; set; }
    }
}