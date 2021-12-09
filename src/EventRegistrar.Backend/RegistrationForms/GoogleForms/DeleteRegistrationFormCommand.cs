using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class DeleteRegistrationFormCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationFormId { get; set; }
}

public class DeleteRegistrationFormCommandHandler : IRequestHandler<DeleteRegistrationFormCommand>
{
    private readonly IQueryable<Event> _events;
    private readonly IRepository<RegistrationForm> _forms;
    private readonly IRepository<Question> _questions;

    public DeleteRegistrationFormCommandHandler(IQueryable<Event> events,
                                                IRepository<RegistrationForm> forms,
                                                IRepository<Question> questions)
    {
        _events = events;
        _forms = forms;
        _questions = questions;
    }

    public async Task<Unit> Handle(DeleteRegistrationFormCommand command, CancellationToken cancellationToken)
    {
        var @event = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (@event.State != State.Setup)
            throw new Exception(
                $"To delete a registration form, event must be in state Setup, but it is in state {@event.State}");

        var form = await _forms.FirstAsync(rbl => rbl.Id == command.RegistrationFormId, cancellationToken);
        if (form.State != State.Setup)
            throw new Exception(
                $"To delete a registration form, it must be in state Setup, but it is in state {@event.State}");

        //var rawForms = await _rawForms.Where(rfm => rfm.FormExternalIdentifier == form.ExternalIdentifier)
        //                              .ToListAsync(cancellationToken);

        _questions.Remove(qst => qst.RegistrationFormId == form.Id);
        _forms.Remove(form);
        return Unit.Value;
    }
}