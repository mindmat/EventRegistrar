using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.Reductions
{
    public class SaveReductionCommand : IRequest, IEventBoundRequest
    {
        public Guid RegistrableId { get; set; }
        public Guid ReductionId { get; set; }
        public decimal Amount { get; set; }
        public Guid? RegistrableId1_ReductionActivatedIfCombinedWith { get; set; }
        public Guid? RegistrableId2_ReductionActivatedIfCombinedWith { get; set; }
        public Guid EventId { get; set; }
    }

    public class SaveReductionCommandHandler : IRequestHandler<SaveReductionCommand>
    {
        private readonly IRepository<Reduction> _reductions;

        public SaveReductionCommandHandler(IRepository<Reduction> reductions)
        {
            _reductions = reductions;
        }

        public async Task<Unit> Handle(SaveReductionCommand command, CancellationToken cancellationToken)
        {
            var reduction = await _reductions.FirstOrDefaultAsync(red => red.Id == command.ReductionId)
                ?? new Reduction
                {
                    Id = command.ReductionId,
                    RegistrableId = command.RegistrableId
                };
            if (reduction.RegistrableId != command.RegistrableId)
            {
                throw new ArgumentException($"Invalid data, reduction {command.ReductionId} does not belong to registrable {command.RegistrableId}");
            }

            reduction.Amount = command.Amount;
            reduction.RegistrableId1_ReductionActivatedIfCombinedWith = command.RegistrableId1_ReductionActivatedIfCombinedWith;
            reduction.RegistrableId2_ReductionActivatedIfCombinedWith = command.RegistrableId2_ReductionActivatedIfCombinedWith;

            await _reductions.InsertOrUpdateEntity(reduction);

            return Unit.Value;
        }
    }
}
