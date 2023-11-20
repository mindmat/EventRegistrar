using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Import;

public class TryAssignImportedMailCommand : IRequest
{
    public Guid ImportedMailId { get; set; }
}

public class TryAssignImportedMailCommandHandler(IQueryable<ImportedMail> importedMails,
                                                 IRepository<ImportedMailToRegistration> mailToRegistrations,
                                                 IQueryable<Registration> _registrations,
                                                 IEventBus eventBus)
    : IRequestHandler<TryAssignImportedMailCommand>
{
    public async Task Handle(TryAssignImportedMailCommand command, CancellationToken cancellationToken)
    {
        var mail = await importedMails.Include(ml => ml.Registrations)
                                      .FirstAsync(ml => ml.Id == command.ImportedMailId, cancellationToken);
        var existingRegistrationMappings = mail.Registrations!.Select(reg => reg.RegistrationId).ToList();
        var emailAddresses = new List<string>(mail.Recipients?.Split(";")) { mail.SenderMail };
        foreach (var emailAddress in emailAddresses)
        {
            var registrations = await _registrations.Where(reg => reg.EventId == mail.EventId
                                                               && reg.RespondentEmail == emailAddress
                                                               && !existingRegistrationMappings.Contains(reg.Id))
                                                    .Select(reg => new { reg.Id, reg.EventId })
                                                    .ToListAsync(cancellationToken);
            foreach (var registration in registrations)
            {
                await mailToRegistrations.InsertOrUpdateEntity(
                    new ImportedMailToRegistration { ImportedMailId = mail.Id, RegistrationId = registration.Id },
                    cancellationToken);
                eventBus.Publish(new ImportedMailAssigned
                                 {
                                     EventId = registration.EventId,
                                     ImportedMailId = mail.Id,
                                     RegistrationId = registration.Id,
                                     ExternalDate = mail.Date,
                                     SenderMail = mail.SenderMail,
                                     SenderName = mail.SenderName
                                 });
            }
        }
    }
}