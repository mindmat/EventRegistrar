using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class SaveRegistrationFormDefinitionCommandHandler : IRequestHandler<SaveRegistrationFormDefinitionCommand>
    {
        private readonly IRepository<RegistrationForm> _forms;

        public SaveRegistrationFormDefinitionCommandHandler(IRepository<RegistrationForm> forms)
        {
            _forms = forms;
        }

        public async Task<Unit> Handle(SaveRegistrationFormDefinitionCommand command, CancellationToken cancellationToken)
        {
            if (command.Description == null)
            {
                throw new ArgumentException("");
            }

            var form = await _forms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == command.FormId, cancellationToken);
            if (form != null)
            {
                // update existing form
                if (form.State != State.Setup)
                {
                    throw new InvalidOperationException("Registration form can only be changed in state 'setup'");
                }

                form.Title = command.Description.Title;
            }
            else
            {
                // new form
                form = new RegistrationForm
                {
                    Id = Guid.NewGuid(),
                    ExternalIdentifier = command.FormId,
                    Title = command.Description.Title,
                    State = State.Setup
                };
                await _forms.InsertOrUpdateEntity(form, cancellationToken);
            }

            return Unit.Value;
        }
    }
}