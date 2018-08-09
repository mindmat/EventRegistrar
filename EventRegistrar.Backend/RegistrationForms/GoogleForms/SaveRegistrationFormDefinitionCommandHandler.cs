using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class SaveRegistrationFormDefinitionCommandHandler : IRequestHandler<SaveRegistrationFormDefinitionCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IRepository<RegistrationForm> _forms;
        private readonly IRepository<RawRegistrationForm> _rawForms;

        public SaveRegistrationFormDefinitionCommandHandler(IRepository<RegistrationForm> forms,
                                                            IRepository<RawRegistrationForm> rawForms,
                                                            IEventAcronymResolver acronymResolver)
        {
            _forms = forms;
            _rawForms = rawForms;
            _acronymResolver = acronymResolver;
        }

        public async Task<Unit> Handle(SaveRegistrationFormDefinitionCommand command, CancellationToken cancellationToken)
        {
            var rawForm = await _rawForms.Where(frm => frm.EventAcronym == command.EventAcronym
                                                    && frm.FormExternalIdentifier == command.FormId
                                                    && !frm.Processed)
                                         .OrderByDescending(frm => frm.Created)
                                         .FirstOrDefaultAsync(cancellationToken);
            if (rawForm == null)
            {
                throw new ArgumentException("No unprocessed form found");
            }

            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);

            var formDescription = JsonConvert.DeserializeObject<FormDescription>(rawForm.ReceivedMessage);
            var form = await _forms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == command.FormId, cancellationToken);
            if (form != null)
            {
                // update existing form
                if (form.State != State.Setup)
                {
                    throw new InvalidOperationException("Registration form can only be changed in state 'setup'");
                }

                form.Title = formDescription.Title;
            }
            else
            {
                // new form
                form = new RegistrationForm
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    ExternalIdentifier = command.FormId,
                    Title = formDescription.Title,
                    State = State.Setup
                };
                await _forms.InsertOrUpdateEntity(form, cancellationToken);
            }

            return Unit.Value;
        }
    }
}