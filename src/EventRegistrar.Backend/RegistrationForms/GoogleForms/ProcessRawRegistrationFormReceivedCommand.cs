using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class ProcessRawRegistrationFormReceivedCommand : IRequest
{
    public Guid RawRegistrationFormId { get; set; }
}

internal class ProcessRawRegistrationFormReceivedCommandHandler(IQueryable<RawRegistrationForm> rawRegistrationForms,
                                                                IEventAcronymResolver eventAcronymResolver,
                                                                ChangeTrigger changeTrigger)
    : IRequestHandler<ProcessRawRegistrationFormReceivedCommand>
{
    public async Task Handle(ProcessRawRegistrationFormReceivedCommand command, CancellationToken cancellationToken)
    {
        var eventId = await GetEventIdOfRawRegistrationForm(command.RawRegistrationFormId, cancellationToken);
        changeTrigger.QueryChanged<EventSetupStateQuery>(eventId);
        changeTrigger.QueryChanged<RegistrationFormsQuery>(eventId);
    }

    private async Task<Guid> GetEventIdOfRawRegistrationForm(Guid rawRegistrationFormId, CancellationToken cancellationToken)
    {
        var eventAcronym = await rawRegistrationForms.Where(rrf => rrf.Id == rawRegistrationFormId)
                                                     .Select(rrf => rrf.EventAcronym)
                                                     .FirstAsync(cancellationToken);
        return await eventAcronymResolver.GetEventIdFromAcronym(eventAcronym);
    }
}