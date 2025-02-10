using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Mailing.Compose;

public class AddIcsToMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailId { get; set; }
}

public class AddIcsToMailCommandHandler(IRepository<Mail> mails,
                                        IRepository<MailAttachment> attachments,
                                        IcsCreator icsCreator,
                                        ChangeTrigger changeTrigger)
    : IRequestHandler<AddIcsToMailCommand>
{
    public async Task Handle(AddIcsToMailCommand command, CancellationToken cancellationToken)
    {
        var mail = await mails.AsTracking()
                              .Include(mail => mail.Attachments!)
                              .Include(mail => mail.Registrations!)
                              .FirstAsync(mail => mail.Id == command.MailId, cancellationToken);
        if (mail.Attachments!.Any(mat => mat.ContentType == "text/calendar"))
        {
            return;
        }

        foreach (var registration in mail.Registrations!)
        {
            var ics = await icsCreator.Create(registration.RegistrationId, cancellationToken);
            if (ics != null)
            {
                ics.MailId = mail.Id;
                attachments.InsertObjectTree(ics);
            }
        }

        if (mail.EventId != null)
        {
            changeTrigger.QueryChanged<MailViewQuery>(mail.EventId.Value, mail.Id);
        }
    }
}