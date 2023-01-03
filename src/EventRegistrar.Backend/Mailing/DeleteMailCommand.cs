using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Mailing;

public class DeleteMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailId { get; set; }
}

public class DeleteMailCommandHandler : AsyncRequestHandler<DeleteMailCommand>
{
    private readonly IRepository<Mail> _mails;
    private readonly ReadModelUpdater _readModelUpdater;

    public DeleteMailCommandHandler(IRepository<Mail> mails,
                                    ReadModelUpdater readModelUpdater)
    {
        _mails = mails;
        _readModelUpdater = readModelUpdater;
    }

    protected override async Task Handle(DeleteMailCommand command, CancellationToken cancellationToken)
    {
        var mailToDelete = await _mails.Where(mail => mail.Id == command.MailId)
                                       .Include(mail => mail.Registrations)
                                       .FirstAsync(cancellationToken);
        mailToDelete.Discarded = true;

        foreach (var registrationId in mailToDelete.Registrations!.Select(reg => reg.RegistrationId))
        {
            _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registrationId, command.EventId);
        }
    }
}