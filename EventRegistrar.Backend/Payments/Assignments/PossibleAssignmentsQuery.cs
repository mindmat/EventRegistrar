using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PossibleAssignmentsQuery : IRequest<IEnumerable<PossibleAssignment>>, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid PaymentId { get; set; }
    }
}