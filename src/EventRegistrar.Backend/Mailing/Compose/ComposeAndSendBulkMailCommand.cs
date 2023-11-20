using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Registrations;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Mailing.Compose;

public class ComposeAndSendBulkMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool AllowDuplicate { get; set; }
    public string? BulkMailKey { get; set; }
    public MailType? MailType { get; set; }
    public bool Withhold { get; set; }
    public object? Data { get; set; }
}

public class ComposeAndSendBulkMailCommandHandler(IQueryable<BulkMailTemplate> templates,
                                                  IQueryable<Registration> registrations,
                                                  IRepository<Mail> mails,
                                                  IRepository<MailToRegistration> mailsToRegistrations,
                                                  MailComposer mailComposer,
                                                  MailConfiguration configuration,
                                                  CommandQueue commandQueue,
                                                  ILogger log,
                                                  IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ComposeAndSendBulkMailCommand>
{
    public const string FallbackLanguage = Language.English;

    public async Task Handle(ComposeAndSendBulkMailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.BulkMailKey))
        {
            throw new ArgumentNullException(nameof(command.BulkMailKey));
        }

        string? dataTypeFullName = null;
        string? dataJson = null;
        if (command.Data != null)
        {
            try
            {
                dataTypeFullName = command.Data.GetType().FullName;
                dataJson = JsonConvert.SerializeObject(command.Data);
            }
            finally { }
        }

        if (!command.AllowDuplicate)
        {
            var duplicate = await mails.Where(ml => ml.Type == command.MailType
                                                 && !ml.Discarded
                                                 && ml.Registrations!.Any(map => map.RegistrationId == command.RegistrationId))
                                       .WhereIf(dataJson != null && dataTypeFullName != null,
                                                ml => ml.DataTypeFullName == dataTypeFullName && ml.DataJson == dataJson)
                                       .FirstOrDefaultAsync(cancellationToken);
            if (duplicate != null)
            {
                log.LogWarning("No mail created because Mail with type {0} found (Id {1})", command.MailType,
                               duplicate.Id);
                return;
            }
        }

        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
        var templatesOfEvent = await templates.Where(mtp => mtp.EventId == registration.EventId)
                                              .WhereIf(command.BulkMailKey != null,
                                                       mtp => mtp.BulkMailKey == command.BulkMailKey)
                                              .ToListAsync(cancellationToken);
        var language = registration.Language ?? FallbackLanguage;
        var template = templatesOfEvent.FirstOrDefault(mtp => mtp.Language == language)
                    ?? templatesOfEvent.FirstOrDefault(mtp => mtp.Language == FallbackLanguage)
                    ?? templatesOfEvent.FirstOrDefault();
        if (template == null)
        {
            throw new ArgumentException($"No template in event {registration.EventId} with type {command.MailType}");
        }

        var partnerRegistration = registration.RegistrationId_Partner.HasValue
                                      ? await registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner.Value,
                                                                                cancellationToken)
                                      : null;

        var content = await mailComposer.Compose(command.RegistrationId,
                                                 template.ContentHtml,
                                                 language,
                                                 cancellationToken);

        var mappings = new List<Registration> { registration };
        if (registration.RegistrationId_Partner != null
         && partnerRegistration != null
         && command.MailType != MailType.OptionsForRegistrationsOnWaitingList
         && command.MailType != MailType.RegistrationCancelled
         && command.BulkMailKey == null) // bulk mails are personal
        {
            mappings.Add(partnerRegistration);
        }

        var mail = new Mail
                   {
                       Id = Guid.NewGuid(),
                       EventId = registration.EventId,
                       //BulkMailTemplateId = template.Id, // ToDo
                       Type = command.MailType,
                       BulkMailKey = command.BulkMailKey,
                       SenderMail = template.SenderMail ?? configuration.SenderMail,
                       SenderName = template.SenderName ?? configuration.SenderName,
                       Subject = template.Subject,
                       Recipients = mappings.Select(reg => reg.RespondentEmail?.ToLowerInvariant())
                                            .Distinct()
                                            .StringJoin(";"),
                       ContentHtml = content,
                       Withhold = command.Withhold,
                       Created = dateTimeProvider.Now
                   };
        if (command.Data != null)
        {
            try
            {
                mail.DataTypeFullName = dataTypeFullName;
                mail.DataJson = dataJson;
            }
            finally { }
        }

        mails.InsertObjectTree(mail);
        foreach (var mailToRegistration in mappings.Select(reg => new MailToRegistration
                                                                  {
                                                                      Id = Guid.NewGuid(),
                                                                      MailId = mail.Id,
                                                                      RegistrationId = reg.Id
                                                                  }))
        {
            mailsToRegistrations.InsertObjectTree(mailToRegistration);
        }

        var sendMailCommand = new SendMailCommand
                              {
                                  EventId = mail.EventId!.Value,
                                  MailId = mail.Id,
                                  ContentHtml = mail.ContentHtml,
                                  ContentPlainText = mail.ContentPlainText,
                                  Subject = mail.Subject,
                                  Sender = new EmailAddress
                                           {
                                               Email = mail.SenderMail,
                                               Name = mail.SenderName
                                           },
                                  To = mappings.Select(reg =>
                                                           new EmailAddress
                                                           {
                                                               Email = reg.RespondentEmail,
                                                               Name = reg.RespondentFirstName
                                                           })
                                               .ToList()
                              };

        if (!command.Withhold)
        {
            mail.Sent = dateTimeProvider.Now;
            commandQueue.EnqueueCommand(sendMailCommand);
        }

        // ToDo
        //foreach (var registrable in registrablesToCheckWaitingList)
        //{
        //    await _serviceBusClient.SendCommand(new TryPromoteFromWaitingListCommand { RegistrableId = registrable.Id }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
        //}
    }
}