using System;
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files.Slips
{
    public class PaymentSlipImageQuery : IRequest<FileContentResult>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid PaymentSlipId { get; set; }
    }
}