using System;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations.Register;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.FormPaths
{
    public class SaveFormPathsCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationFormId { get; set; }
        public IRegistrationProcessConfiguration Configuration { get; set; } = null!;
    }

    public class SaveFormPathsCommandHandler : IRequestHandler<SaveFormPathsCommand>
    {
        private readonly IRepository<FormPath> _formPaths;

        public SaveFormPathsCommandHandler(IRepository<FormPath> formPaths)
        {
            _formPaths = formPaths;
        }

        public async Task<Unit> Handle(SaveFormPathsCommand command, CancellationToken cancellationToken)
        {
            var type = FormPathType.Single;
            if (command.Configuration is PartnerRegistrationProcessConfiguration)
            {
                type = FormPathType.Partner;
            }

            var formPath = await _formPaths.FirstOrDefaultAsync(fpt => fpt.Id == command.Configuration.Id, cancellationToken)
                           ?? new FormPath
                           {
                               Id = command.Configuration.Id,
                           };

            formPath.RegistrationFormId = command.Configuration.RegistrationFormId;
            formPath.Description = command.Configuration.Description;
            formPath.Type = type;
            formPath.Configuration = command.Configuration;
            formPath.RegistrationFormId = command.RegistrationFormId;

            await _formPaths.InsertOrUpdateEntity(formPath, cancellationToken);

            return Unit.Value;
        }
    }
}