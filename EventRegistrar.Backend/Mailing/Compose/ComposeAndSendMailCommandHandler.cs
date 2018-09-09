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

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ComposeAndSendMailCommandHandler : IRequestHandler<ComposeAndSendMailCommand>
    {
        public const string FallbackLanguage = Language.Deutsch;

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
                                                ServiceBusClient serviceBusClient)
        {
            _templates = templates;
            _registrations = registrations;
            _mails = mails;
            _mailsToRegistrations = mailsToRegistrations;
            _mailComposer = mailComposer;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(ComposeAndSendMailCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
            var templates = await _templates.Where(mtp => mtp.EventId == registration.EventId &&
                                                          mtp.Type == command.MailType)
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
            if (registration.RegistrationId_Partner.HasValue && partnerRegistration != null)
            {
                mappings.Add(partnerRegistration);
            }

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
                Recipients = string.Join(";", mappings.Select(reg => reg.RespondentEmail)),
                Withhold = command.Withhold,
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

            if (!command.Withhold)
            {
                await _serviceBusClient.SendCommand(sendMailCommand);
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