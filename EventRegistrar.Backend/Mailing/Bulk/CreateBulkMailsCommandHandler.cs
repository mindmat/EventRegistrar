﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.Bulk
{
    public class CreateBulkMailsCommandHandler : IRequestHandler<CreateBulkMailsCommand>
    {
        private readonly MailComposer _mailComposer;
        private readonly IRepository<Mail> _mails;
        private readonly IRepository<MailToRegistration> _mailsToRegistrations;
        private readonly IQueryable<MailTemplate> _mailTemplates;
        private readonly IQueryable<Registration> _registrations;

        public CreateBulkMailsCommandHandler(IQueryable<MailTemplate> mailTemplates,
                                             IQueryable<Registration> registrations,
                                             IRepository<Mail> mails,
                                             IRepository<MailToRegistration> mailsToRegistrations,
                                             MailComposer mailComposer)
        {
            _mailTemplates = mailTemplates;
            _registrations = registrations;
            _mails = mails;
            _mailsToRegistrations = mailsToRegistrations;
            _mailComposer = mailComposer;
        }

        public async Task<Unit> Handle(CreateBulkMailsCommand command, CancellationToken cancellationToken)
        {
            var templates = await _mailTemplates.Where(mtp => mtp.EventId == command.EventId
                                                           && mtp.BulkMailKey == command.BulkMailKey
                                                           && mtp.Type == 0
                                                           && !mtp.IsDeleted)
                                                .ToListAsync(cancellationToken);

            var registrationsOfEvent = await _registrations.Where(reg => reg.EventId == command.EventId
                                                                      && reg.State != RegistrationState.Cancelled
                                                                      && !reg.Mails.Any(mail => mail.Mail.BulkMailKey == command.BulkMailKey))
                                                           .Include(reg => reg.Seats_AsLeader)
                                                           .Include(reg => reg.Seats_AsFollower)
                                                           .ToListAsync(cancellationToken);
            foreach (var mailTemplate in templates)
            {
                var registrationsForTemplate = mailTemplate.RegistrableId == null
                    ? registrationsOfEvent
                    : registrationsOfEvent.Where(reg => reg.Seats_AsLeader.Any(spt => !spt.IsCancelled && spt.RegistrableId == mailTemplate.RegistrableId)
                                                     || reg.Seats_AsFollower.Any(spt => !spt.IsCancelled && spt.RegistrableId == mailTemplate.RegistrableId))
                                          .Take(100)
                                          .ToList();
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Paid) == true)
                {
                    var receivers = registrationsForTemplate.Where(reg => reg.State == RegistrationState.Paid
                                                                       && (reg.Language == mailTemplate.Language || reg.Language == null));
                    foreach (var registration in receivers)
                    {
                        await CreateMail(mailTemplate, registration, cancellationToken);
                    }
                }
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.Unpaid) == true)
                {
                    var receivers = registrationsForTemplate.Where(reg => reg.State == RegistrationState.Received
                                                                       && (reg.Language == mailTemplate.Language || reg.Language == null)
                                                                       && reg.IsWaitingList != true);
                    foreach (var registration in receivers)
                    {
                        await CreateMail(mailTemplate, registration, cancellationToken);
                    }
                }
                if (mailTemplate.MailingAudience?.HasFlag(MailingAudience.WaitingList) == true)
                {
                    var receivers = registrationsForTemplate.Where(reg => reg.State == RegistrationState.Received
                                                                       && (reg.Language == mailTemplate.Language || reg.Language == null)
                                                                       && reg.IsWaitingList == true);
                    foreach (var registration in receivers)
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