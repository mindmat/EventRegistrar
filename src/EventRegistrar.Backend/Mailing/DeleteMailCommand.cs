using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;

using MediatR;

namespace EventRegistrar.Backend.Mailing;

public class DeleteMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailId { get; set; }
}

public class DeleteMailCommandHandler : IRequestHandler<DeleteMailCommand>
{
    private readonly IRepository<Mail> _mails;
    private readonly ReadModelUpdater _readModelUpdater;

    public DeleteMailCommandHandler(IRepository<Mail> mails,
                                    ReadModelUpdater readModelUpdater)
    {
        _mails = mails;
        _readModelUpdater = readModelUpdater;
    }

    public async Task<Unit> Handle(DeleteMailCommand command, CancellationToken cancellationToken)
    {
        var mailToDelete = await _mails.FirstAsync(mail => mail.Id == command.MailId, cancellationToken);
        mailToDelete.Discarded = true;

        _readModelUpdater.TriggerUpdate<RegistrationCalculator>();

        return Unit.Value;
    }
}