using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class TryAssignImportedMailCommand : IRequest
    {
        public Guid ImportedMailId { get; set; }
    }

    public class TryAssignImportedMailCommandHandler : IRequestHandler<TryAssignImportedMailCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly IQueryable<ImportedMail> _importedMails;
        private readonly IRepository<ImportedMailToRegistration> _mailToRegistrations;
        private readonly IQueryable<Registration> _registrations;

        public TryAssignImportedMailCommandHandler(IQueryable<ImportedMail> importedMails,
                                                   IRepository<ImportedMailToRegistration> mailToRegistrations,
                                                   IQueryable<Registration> registrations,
                                                   IEventBus eventBus)
        {
            _importedMails = importedMails;
            _mailToRegistrations = mailToRegistrations;
            _registrations = registrations;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(TryAssignImportedMailCommand command, CancellationToken cancellationToken)
        {
            var mail = await _importedMails.Include(ml => ml.Registrations)
                                           .FirstAsync(ml => ml.Id == command.ImportedMailId, cancellationToken);
            var existingRegistrationMappings = mail.Registrations.Select(reg => reg.RegistrationId).ToList();
            var emailAddresses = new List<string>(mail.Recipients?.Split(";")) { mail.SenderMail };
            foreach (var emailAddress in emailAddresses)
            {
                var registrations = await _registrations.Where(reg => reg.EventId == mail.EventId
                                                                   && reg.RespondentEmail == emailAddress
                                                                   && !existingRegistrationMappings.Contains(reg.Id))
                                                        .Select(reg => reg.Id)
                                                        .ToListAsync(cancellationToken);
                foreach (var registration in registrations)
                {
                    await _mailToRegistrations.InsertOrUpdateEntity(new ImportedMailToRegistration { ImportedMailId = mail.Id, RegistrationId = registration }, cancellationToken);
                    _eventBus.Publish(new ImportedMailAssigned
                    {
                        ImportedMailId = mail.Id,
                        RegistrationId = registration,
                        ExternalDate = mail.Date
                    });
                }
            }

            return Unit.Value;
        }
    }
}