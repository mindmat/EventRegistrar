using System;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
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
        private readonly JsonHelper _jsonHelper;

        public SaveFormPathsCommandHandler(IRepository<FormPath> formPaths,
                                           JsonHelper jsonHelper)
        {
            _formPaths = formPaths;
            _jsonHelper = jsonHelper;
        }

        public async Task<Unit> Handle(SaveFormPathsCommand command, CancellationToken cancellationToken)
        {

            var formPath = await _formPaths.FirstOrDefaultAsync(fpt => fpt.Id == command.Configuration.Id, cancellationToken)
                           ?? new FormPath
                           {
                               Id = command.Configuration.Id,
                           };

            formPath.RegistrationFormId = command.Configuration.RegistrationFormId;
            formPath.Description = command.Configuration.Description;
            if (command.Configuration is SingleRegistrationProcessConfiguration singleConfig)
            {
                formPath.ConfigurationJson = _jsonHelper.Serialize(command.Configuration);
                formPath.Type = FormPathType.Single;
            }
            else if (command.Configuration is PartnerRegistrationProcessConfiguration partnerConfig)
            {
                formPath.ConfigurationJson = _jsonHelper.Serialize(command.Configuration);
                formPath.Type = FormPathType.Partner;
            }

            formPath.RegistrationFormId = command.RegistrationFormId;

            await _formPaths.InsertOrUpdateEntity(formPath, cancellationToken);

            return Unit.Value;
        }
    }
}