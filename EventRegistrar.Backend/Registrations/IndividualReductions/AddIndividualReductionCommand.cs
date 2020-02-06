using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.IndividualReductions
{
    public class AddIndividualReductionCommand : IRequest, IEventBoundRequest
    {
        public decimal Amount { get; set; }
        public Guid EventId { get; set; }
        public string Reason { get; set; }
        public Guid ReductionId { get; set; }
        public Guid RegistrationId { get; set; }
    }

    public class AddIndividualReductionCommandHandler : IRequestHandler<AddIndividualReductionCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly IRepository<IndividualReduction> _reductions;
        private readonly IQueryable<Registration> _registrations;
        private readonly AuthenticatedUserId _userId;

        public AddIndividualReductionCommandHandler(IQueryable<Registration> registrations,
                                                    IRepository<IndividualReduction> reductions,
                                                    AuthenticatedUserId userId,
                                                    IEventBus eventBus)
        {
            _registrations = registrations;
            _reductions = reductions;
            _userId = userId;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(AddIndividualReductionCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == command.EventId, cancellationToken);

            var reduction = new IndividualReduction
            {
                Id = command.ReductionId,
                RegistrationId = command.RegistrationId,
                Amount = command.Amount,
                Reason = command.Reason,
                UserId = _userId.UserId.Value
            };
            await _reductions.InsertOrUpdateEntity(reduction, cancellationToken);

            _eventBus.Publish(new IndividualReductionAdded
            {
                RegistrationId = registration.Id,
                Amount = command.Amount,
                Reason = command.Reason
            });
            return Unit.Value;
        }
    }
}