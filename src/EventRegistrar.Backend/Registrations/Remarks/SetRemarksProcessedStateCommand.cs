using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class SetRemarksProcessedStateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RemarkId { get; set; }
    public bool NewProcessedState { get; set; }
}

public class SetRemarksProcessedStateCommandHandler(IRepository<RegistrationRemark> remarks,
                                                    ChangeTrigger changeTrigger)
    : IRequestHandler<SetRemarksProcessedStateCommand>
{
    public async Task Handle(SetRemarksProcessedStateCommand command, CancellationToken cancellationToken)
    {
        var remark = await remarks.AsTracking()
                                  .FirstAsync(rmk => rmk.Id == command.RemarkId
                                                  && rmk.Registration!.EventId == command.EventId,
                                              cancellationToken);

        if (remark.Processed != command.NewProcessedState)
        {
            remark.Processed = command.NewProcessedState;

            changeTrigger.TriggerUpdate<RemarksOverviewCalculator>(null, command.EventId);
            changeTrigger.TriggerUpdate<RegistrationCalculator>(remark.RegistrationId, command.EventId);
        }
    }
}