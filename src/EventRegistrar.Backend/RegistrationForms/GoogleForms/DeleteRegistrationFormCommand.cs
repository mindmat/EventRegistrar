using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.RegistrationForms.Questions;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class DeleteRegistrationFormCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationFormId { get; set; }
}

public class DeleteRegistrationFormCommandHandler(IQueryable<Event> events,
                                                  IRepository<RegistrationForm> forms,
                                                  IRepository<Question> questions)
    : IRequestHandler<DeleteRegistrationFormCommand>
{
    public async Task Handle(DeleteRegistrationFormCommand command, CancellationToken cancellationToken)
    {
        var @event = await events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (@event.State != EventState.Setup)
        {
            throw new Exception(
                $"To delete a registration form, event must be in state Setup, but it is in state {@event.State}");
        }

        var form = await forms.FirstAsync(rbl => rbl.Id == command.RegistrationFormId, cancellationToken);
        if (form.State != EventState.Setup)
        {
            throw new Exception(
                $"To delete a registration form, it must be in state Setup, but it is in state {@event.State}");
        }

        //var rawForms = await _rawForms.Where(rfm => rfm.FormExternalIdentifier == form.ExternalIdentifier)
        //                              .ToListAsync(cancellationToken);

        questions.Remove(qst => qst.RegistrationFormId == form.Id);
        forms.Remove(form);
    }
}