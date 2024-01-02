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
                mailToRegistrations.InsertObjectTree(new ImportedMailToRegistration
                                                     {
                                                         Id = Guid.NewGuid(),
                                                         ImportedMailId = mail.Id,
                                                         RegistrationId = registration.Id
                                                     });
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