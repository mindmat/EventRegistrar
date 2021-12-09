using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Import;

public class TryAssignWhenExternalMailImported : IEventToCommandTranslation<ExternalMailImported>
{
    public IEnumerable<IRequest> Translate(ExternalMailImported e)
    {
        yield return new TryAssignImportedMailCommand { ImportedMailId = e.ImportedMailId };
    }
}