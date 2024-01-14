using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Mailing.Compose;

public class ComposeAndSendAutoMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool AllowDuplicate { get; set; }
    public MailType MailType { get; set; }
    public Guid RegistrationId { get; set; }
    public bool Withhold { get; set; }
    public object? Data { get; set; }
}

public class ComposeAndSendAutoMailCommandHandler(IQueryable<AutoMailTemplate> templates,
                                                  IQueryable<Registration> registrations,
                                                  IRepository<Mail> mails,
                                                  IRepository<MailToRegistration> mailsToRegistrations,
                                                  MailConfiguration configuration,
                                                  MailComposer mailComposer,
                                                  CommandQueue commandQueue,
                                                  IDateTimeProvider dateTimeProvider,
                                                  ILogger log,
                                                  ChangeTrigger changeTrigger,
                                                  DirtyTagger dirtyTagger,
                                                  IEventBus eventBus)
    : IRequestHandler<ComposeAndSendAutoMailCommand>
{
    public const string FallbackLanguage = Language.English;

    public async Task Handle(ComposeAndSendAutoMailCommand command, CancellationToken cancellationToken)
    {
        await dirtyTagger.WaitForRemovedTags(command.RegistrationId, typeof(RegistrationPriceAndWaitingListSegment));

        string? dataTypeFullName = null;
        string? dataJson = null;
        if (command.Data != null)
        {
            try
            {
                dataTypeFullName = command.Data.GetType().FullName!;
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
                log.LogWarning("No mail created because Mail with type {0} found (Id {1})",
                               command.MailType,
                               duplicate.Id);
                return;
            }
        }

        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);
        var templatesOfEvent = await templates.Where(mtp => mtp.EventId == command.EventId)
                                              .Where(mtp => mtp.Type == command.MailType)
                                              .ToListAsync(cancellationToken);
        var language = registration.Language ?? configuration.FallbackLanguage ?? FallbackLanguage;
        var template = templatesOfEvent.FirstOrDefault(mtp => mtp.Language == language)
                    ?? templatesOfEvent.FirstOrDefault(mtp => mtp.Language == FallbackLanguage)
                    ?? templatesOfEvent.FirstOrDefault();
        if (template?.ContentHtml == null)
        {
            throw new ArgumentException($"No template in event {registration.EventId} with type {command.MailType}");
        }

        var partnerRegistration = registration.RegistrationId_Partner != null
                                      ? await registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner
                                                                                    && reg.EventId == command.EventId,
                                                                                cancellationToken)
                                      : null;

        var content = await mailComposer.Compose(command.RegistrationId, template.ContentHtml, language, cancellationToken);

        var registrations_Recipients = new List<Registration> { registration };
        if (registration.RegistrationId_Partner != null
         && partnerRegistration != null
         && command.MailType != MailType.OptionsForRegistrationsOnWaitingList
         && command.MailType != MailType.RegistrationCancelled)
        {
            registrations_Recipients.Add(partnerRegistration);
        }

        var withhold = !template.ReleaseImmediately
                    || command.Withhold;
        var mail = new Mail
                   {
                       Id = Guid.NewGuid(),
                       EventId = registration.EventId,
                       AutoMailTemplateId = template.Id,
                       Type = command.MailType,
                       SenderMail = configuration.SenderMail,
                       SenderName = configuration.SenderName,
                       Subject = template.Subject,
                       Recipients = registrations_Recipients.Select(reg => reg.RespondentEmail?.ToLowerInvariant())
                                                            .Distinct()
                                                            .StringJoinNullable(";"),
                       Withhold = withhold,
                       Created = dateTimeProvider.Now,
                       ContentHtml = content
                   };

        mails.InsertObjectTree(mail);
        foreach (var mailToRegistration in registrations_Recipients.Select(reg => new MailToRegistration
                                                                                  {
                                                                                      Id = Guid.NewGuid(),
                                                                                      MailId = mail.Id,
                                                                                      Email = reg.RespondentEmail?.ToLowerInvariant(),
                                                                                      RegistrationId = reg.Id
                                                                                  }))
        {
            await mailsToRegistrations.InsertOrUpdateEntity(mailToRegistration, cancellationToken);
        }

        if (!withhold)
        {
            mail.Sent = dateTimeProvider.Now;
            var sendMailCommand = new SendMailCommand
                                  {
                                      EventId = mail.EventId!.Value,
                                      MailId = mail.Id
                                  };
            commandQueue.EnqueueCommand(sendMailCommand);
        }

        registrations_Recipients.ForEach(reg => changeTrigger.TriggerUpdate<RegistrationCalculator>(reg.Id, reg.EventId));
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);
        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(PendingMailsQuery)
                         });

        // ToDo
        //foreach (var registrable in registrablesToCheckWaitingList)
        //{
        //    await _serviceBusClient.SendCommand(new TryPromoteFromWaitingListCommand { RegistrableId = registrable.Id }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
        //}
    }
}