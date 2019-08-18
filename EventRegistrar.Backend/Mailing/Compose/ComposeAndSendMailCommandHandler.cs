using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ComposeAndSendMailCommandHandler : IRequestHandler<ComposeAndSendMailCommand>
    {
        public const string FallbackLanguage = Language.English;

        private readonly ILogger _log;
        private readonly MailReleaseConfiguration _mailReleaseConfiguration;
        private readonly MailComposer _mailComposer;
        private readonly IRepository<Mail> _mails;
        private readonly IRepository<MailToRegistration> _mailsToRegistrations;
        private readonly IQueryable<Registration> _registrations;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IQueryable<MailTemplate> _templates;

        public ComposeAndSendMailCommandHandler(IQueryable<MailTemplate> templates,
                                                IQueryable<Registration> registrations,
                                                IRepository<Mail> mails,
                                                IRepository<MailToRegistration> mailsToRegistrations,
                                                MailComposer mailComposer,
                                                ServiceBusClient serviceBusClient,
                                                ILogger log,
                                                MailReleaseConfiguration mailReleaseConfiguration)
        {
            _templates = templates;
            _registrations = registrations;
            _mails = mails;
            _mailsToRegistrations = mailsToRegistrations;
            _mailComposer = mailComposer;
            _serviceBusClient = serviceBusClient;
            _log = log;
            _mailReleaseConfiguration = mailReleaseConfiguration;
        }

        public async Task<Unit> Handle(ComposeAndSendMailCommand command, CancellationToken cancellationToken)
        {
            if (!command.AllowDuplicate)
            {
                var duplicate = await _mails.FirstOrDefaultAsync(ml => ml.Type == command.MailType
                                                                    && !ml.Discarded
                                                                    && ml.Registrations.Any(map => map.RegistrationId == command.RegistrationId),
                                                                 cancellationToken);
                if (duplicate != null)
                {
                    _log.LogWarning("No mail created because Mail with type {0} found (Id {1})", command.MailType, duplicate.Id);
                    return Unit.Value;
                }
            }

            var registration = await _registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
            var templates = await _templates.Where(mtp => mtp.EventId == registration.EventId
                                                       && !mtp.IsDeleted)
                                            .WhereIf(command.MailType != null, mtp => mtp.Type == command.MailType)
                                            .WhereIf(command.BulkMailKey != null, mtp => mtp.BulkMailKey == command.BulkMailKey)
                                            .ToListAsync(cancellationToken);
            var language = registration.Language ?? FallbackLanguage;
            var template = templates.FirstOrDefault(mtp => mtp.Language == language) ??
                           templates.FirstOrDefault(mtp => mtp.Language == FallbackLanguage) ??
                           templates.FirstOrDefault();
            if (template == null)
            {
                throw new ArgumentException($"No template in event {registration.EventId} with type {command.MailType}");
            }

            var partnerRegistration = registration.RegistrationId_Partner.HasValue
                ? await _registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner.Value, cancellationToken)
                : null;

            var content = await _mailComposer.Compose(command.RegistrationId, template.Template, language, cancellationToken);

            var mappings = new List<Registration> { registration };
            if (registration.RegistrationId_Partner.HasValue
             && partnerRegistration != null
             && command.MailType != MailType.OptionsForRegistrationsOnWaitingList
             && command.MailType != MailType.RegistrationCancelled
             && command.BulkMailKey == null)  // bulk mails are personal
            {
                mappings.Add(partnerRegistration);
            }

            var releaseAutomatically = command.MailType != null
                                    && _mailReleaseConfiguration.MailsToReleaseAutomatically != null
                                    && _mailReleaseConfiguration.MailsToReleaseAutomatically.Contains(command.MailType.Value) == true;
            var withhold = !releaseAutomatically
                        || command.Withhold;
            var mail = new Mail
            {
                Id = Guid.NewGuid(),
                EventId = registration.EventId,
                MailTemplateId = template.Id,
                Type = command.MailType,
                BulkMailKey = command.BulkMailKey,
                SenderMail = template.SenderMail,
                SenderName = template.SenderName,
                Subject = template.Subject,
                Recipients = mappings.Select(reg => reg.RespondentEmail?.ToLowerInvariant()).Distinct().StringJoin(";"),
                Withhold = withhold,
                Created = DateTime.UtcNow
            };

            if (template.ContentType == MailContentType.Html)
            {
                mail.ContentHtml = content;
            }
            else
            {
                mail.ContentPlainText = content;
            }

            await _mails.InsertOrUpdateEntity(mail, cancellationToken);
            foreach (var mailToRegistration in mappings.Select(reg => new MailToRegistration { Id = Guid.NewGuid(), MailId = mail.Id, RegistrationId = reg.Id }))
            {
                await _mailsToRegistrations.InsertOrUpdateEntity(mailToRegistration, cancellationToken);
            }

            var sendMailCommand = new SendMailCommand
            {
                MailId = mail.Id,
                ContentHtml = mail.ContentHtml,
                ContentPlainText = mail.ContentPlainText,
                Subject = mail.Subject,
                Sender = new EmailAddress { Email = mail.SenderMail, Name = mail.SenderName },
                To = mappings.Select(reg => new EmailAddress { Email = reg.RespondentEmail, Name = reg.RespondentFirstName }).ToList()
            };

            if (!withhold)
            {
                mail.Sent = DateTime.UtcNow;
                _serviceBusClient.SendMessage(sendMailCommand);
            }
            // ToDo
            //foreach (var registrable in registrablesToCheckWaitingList)
            //{
            //    await _serviceBusClient.SendCommand(new TryPromoteFromWaitingListCommand { RegistrableId = registrable.Id }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
            //}
            return Unit.Value;
        }
    }
}