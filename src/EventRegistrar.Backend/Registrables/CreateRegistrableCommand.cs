using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Registrables
{
    public class CreateRegistrableParameters
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDoubleRegistrable { get; set; }
    }

    public class CreateRegistrableCommand : IRequest, IEventBoundRequest
    {
        public CreateRegistrableParameters Parameters { get; set; }

        public Guid EventId { get; set; }
    }

    public class CreateRegistrableCommandHandler : IRequestHandler<CreateRegistrableCommand>
    {
        private readonly IRepository<Registrable> _registrables;

        public CreateRegistrableCommandHandler(IRepository<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<Unit> Handle(CreateRegistrableCommand command, CancellationToken cancellationToken)
        {
            var registrable = new Registrable
            {
                Id = command.Parameters.Id,
                EventId = command.EventId,
                Name = command.Parameters.Name
            };
            await _registrables.InsertOrUpdateEntity(registrable);
            return Unit.Value;
        }
    }
}
