using System;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class SaveRegistrationFormMappingsCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public RegistrationFormMappings? Mappings { get; set; }
    }

    public class SaveRegistrationFormMappingsCommandHandler : IRequestHandler<SaveRegistrationFormMappingsCommand>
    {
        private readonly IRepository<RegistrationForm> _forms;

        public SaveRegistrationFormMappingsCommandHandler(IRepository<RegistrationForm> forms)
        {
            _forms = forms;
        }

        public async Task<Unit> Handle(SaveRegistrationFormMappingsCommand command, CancellationToken cancellationToken)
        {
            var formToSave = command.Mappings;
            if (formToSave == null)
            {
                return Unit.Value;
            }

            var form = await _forms.FirstAsync(frm => frm.Id == formToSave.RegistrationFormId, cancellationToken);

            //form.Type = formToSave.Type ?? form.Type;
            //if (form.Type == FormPathType.Single && formToSave.SingleConfiguration != null)
            //{
            //    form.ProcessConfigurationJson = JsonConvert.SerializeObject(formToSave.SingleConfiguration);
            //}

            return Unit.Value;
        }
    }
}