using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

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

public class ComposeAndSendAutoMailCommandHandler : IRequestHandler<ComposeAndSendAutoMailCommand>
{
    public const string FallbackLanguage = Language.English;

    private readonly ILogger _log;
    private readonly ReadModelUpdater _readModelUpdater;
    private readonly MailComposer _mailComposer;
    private readonly IRepository<Mail> _mails;
    private readonly IRepository<MailToRegistration> _mailsToRegistrations;
    private readonly MailConfiguration _configuration;
    private readonly IQueryable<Registration> _registrations;
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IQueryable<AutoMailTemplate> _templates;

    public ComposeAndSendAutoMailCommandHandler(IQueryable<AutoMailTemplate> templates,
                                                IQueryable<Registration> registrations,
                                                IRepository<Mail> mails,
                                                IRepository<MailToRegistration> mailsToRegistrations,
                                                MailConfiguration configuration,
                                                MailComposer mailComposer,
                                                CommandQueue commandQueue,
                                                IDateTimeProvider dateTimeProvider,
                                                ILogger log,
                                                ReadModelUpdater readModelUpdater)
    {
        _templates = templates;
        _registrations = registrations;
        _mails = mails;
        _mailsToRegistrations = mailsToRegistrations;
        _configuration = configuration;
        _mailComposer = mailComposer;
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _log = log;
        _readModelUpdater = readModelUpdater;
    }

    public async Task<Unit> Handle(ComposeAndSendAutoMailCommand command, CancellationToken cancellationToken)
    {
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
            var duplicate = await _mails.Where(ml => ml.Type == command.MailType
                                                  && !ml.Discarded
                                                  && ml.Registrations!.Any(map => map.RegistrationId == command.RegistrationId))
                                        .WhereIf(dataJson != null && dataTypeFullName != null,
                                                 ml => ml.DataTypeFullName == dataTypeFullName && ml.DataJson == dataJson)
                                        .FirstOrDefaultAsync(cancellationToken);
            if (duplicate != null)
            {
                _log.LogWarning("No mail created because Mail with type {0} found (Id {1})",
                                command.MailType,
                                duplicate.Id);
                return Unit.Value;
            }
        }

        var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);
        var templates = await _templates.Where(mtp => mtp.EventId == command.EventId)
                                        .Where(mtp => mtp.Type == command.MailType)
                                        .ToListAsync(cancellationToken);
        var language = registration.Language ?? _configuration.FallbackLanguage ?? FallbackLanguage;
        var template = templates.FirstOrDefault(mtp => mtp.Language == language)
                    ?? templates.FirstOrDefault(mtp => mtp.Language == FallbackLanguage)
                    ?? templates.FirstOrDefault();
        if (template?.ContentHtml == null)
        {
            throw new ArgumentException($"No template in event {registration.EventId} with type {command.MailType}");
        }

        var partnerRegistration = registration.RegistrationId_Partner != null
                                      ? await _registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner
                                                                                     && reg.EventId == command.EventId,
                                                                                 cancellationToken)
                                      : null;

        var content = await _mailComposer.Compose(command.RegistrationId, template.ContentHtml, language, cancellationToken);

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
                       SenderMail = _configuration.SenderMail,
                       SenderName = _configuration.SenderName,
                       Subject = template.Subject,
                       Recipients = registrations_Recipients.Select(reg => reg.RespondentEmail?.ToLowerInvariant())
                                                            .Distinct()
                                                            .StringJoinNullable(";"),
                       Withhold = withhold,
                       Created = _dateTimeProvider.Now,
                       ContentHtml = content
                   };

        _mails.InsertObjectTree(mail);
        foreach (var mailToRegistration in registrations_Recipients.Select(reg => new MailToRegistration
                                                                                  {
                                                                                      Id = Guid.NewGuid(),
                                                                                      MailId = mail.Id,
                                                                                      RegistrationId = reg.Id
                                                                                  }))
        {
            await _mailsToRegistrations.InsertOrUpdateEntity(mailToRegistration, cancellationToken);
        }

        if (!withhold)
        {
            mail.Sent = _dateTimeProvider.Now;
            var sendMailCommand = new SendMailCommand
                                  {
                                      MailId = mail.Id,
                                      ContentHtml = mail.ContentHtml,
                                      ContentPlainText = mail.ContentPlainText,
                                      Subject = mail.Subject,
                                      Sender = new EmailAddress
                                               {
                                                   Email = mail.SenderMail,
                                                   Name = mail.SenderName
                                               },
                                      To = registrations_Recipients.Select(reg =>
                                                                               new EmailAddress
                                                                               {
                                                                                   Email = reg.RespondentEmail,
                                                                                   Name = reg.RespondentFirstName
                                                                               })
                                                                   .ToList()
                                  };
            _commandQueue.EnqueueCommand(sendMailCommand);
        }

        registrations_Recipients.ForEach(reg => _readModelUpdater.TriggerUpdate<RegistrationCalculator>(reg.Id, reg.EventId));

        // ToDo
        //foreach (var registrable in registrablesToCheckWaitingList)
        //{
        //    await _serviceBusClient.SendCommand(new TryPromoteFromWaitingListCommand { RegistrableId = registrable.Id }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
        //}
        return Unit.Value;
    }
}