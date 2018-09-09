using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class CreateBulkMailsCommandHandler : IRequestHandler<CreateBulkMailsCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly ILogger _log;
        private readonly MailComposer _mailComposer;
        private readonly IRepository<Mail> _mails;
        private readonly IRepository<MailToRegistration> _mailsToRegistrations;
        private readonly IQueryable<MailTemplate> _mailTemplates;
        private readonly IQueryable<Registration> _registrations;

        public CreateBulkMailsCommandHandler(IQueryable<MailTemplate> mailTemplates,
            IQueryable<Registration> registrations,
            IRepository<Mail> mails,
            IRepository<MailToRegistration> mailsToRegistrations,
            IEventAcronymResolver acronymResolver,
            MailComposer mailComposer,
            ILogger log)
        {
            _mailTemplates = mailTemplates;
            _registrations = registrations;
            _mails = mails;
            _mailsToRegistrations = mailsToRegistrations;
            _acronymResolver = acronymResolver;
            _mailComposer = mailComposer;
            _log = log;
        }

        public async Task<Unit> Handle(CreateBulkMailsCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);
            var templates = await _mailTemplates.Where(mtp => mtp.EventId == eventId
                                                              && mtp.BulkMailKey == command.TemplateKey
                                                              && mtp.Type == 0)
                .ToListAsync(cancellationToken);

            foreach (var mailTemplate in templates)
            {
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Paid) == true)
                {
                    var registrations = await _registrations.Where(reg => reg.State == RegistrationState.Paid
                                                                          && reg.RegistrationForm.Language == mailTemplate.Language
                                                                          && !reg.Mails.Any(mail => mail.Mail.BulkMailKey == mailTemplate.BulkMailKey))
                                                            .ToListAsync(cancellationToken);
                    _log.LogInformation($"paid {registrations.Count}");
                    foreach (var registration in registrations)
                    {
                        await CreateMail(mailTemplate, registration, cancellationToken);
                    }
                }
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Unpaid) == true)
                {
                    var registrations = await _registrations.Where(reg => reg.State == RegistrationState.Received
                                                                          && reg.IsWaitingList != true
                                                                          && reg.RegistrationForm.Language == mailTemplate.Language
                                                                          && !reg.Mails.Any(mail => mail.Mail.BulkMailKey == mailTemplate.BulkMailKey))
                                                            .ToListAsync(cancellationToken);
                    _log.LogInformation($"unpaid {registrations.Count}");
                    foreach (var registration in registrations)
                    {
                        await CreateMail(mailTemplate, registration, cancellationToken);
                    }
                }
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.WaitingList) == true)
                {
                    var registrations = await _registrations.Where(reg => reg.State == RegistrationState.Received
                                                                          && reg.IsWaitingList == true
                                                                          && reg.RegistrationForm.Language == mailTemplate.Language
                                                                          && !reg.Mails.Any(mail => mail.Mail.BulkMailKey == mailTemplate.BulkMailKey))
                                                            .ToListAsync(cancellationToken);
                    _log.LogInformation($"waiting list {registrations.Count}");
                    foreach (var registration in registrations)
                    {
                        await CreateMail(mailTemplate, registration, cancellationToken);
                    }
                }
            }

            return Unit.Value;
        }

        private async Task CreateMail(MailTemplate mailTemplate, Registration registration, CancellationToken cancellationToken)
        {
            var content = await _mailComposer.Compose(registration.Id, mailTemplate.Template, mailTemplate.Language, cancellationToken);
            var mail = new Mail
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Recipients = registration.RespondentEmail,
                SenderMail = mailTemplate.SenderMail,
                SenderName = mailTemplate.SenderName,
                Subject = mailTemplate.Subject,
                Withhold = true,
                BulkMailKey = mailTemplate.BulkMailKey,
                ContentHtml = content,
                EventId = registration.EventId,
                MailTemplateId = mailTemplate.Id
            };
            await _mails.InsertOrUpdateEntity(mail, cancellationToken);
            await _mailsToRegistrations.InsertOrUpdateEntity(new MailToRegistration
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                MailId = mail.Id,
            }, cancellationToken);
        }
    }
}