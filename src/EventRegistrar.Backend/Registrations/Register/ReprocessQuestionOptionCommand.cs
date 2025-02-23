using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Registrables.WaitingList.MoveUp;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Register;

public class ReprocessQuestionOptionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid QuestionOptionId { get; set; }
}

public class ReprocessQuestionOptionCommandHandler(IRepository<QuestionOption> questionOptions,
                                                   IRepository<Seat> spots,
                                                   ChangeTrigger changeTrigger)
    : IRequestHandler<ReprocessQuestionOptionCommand>
{
    public async Task Handle(ReprocessQuestionOptionCommand command, CancellationToken cancellationToken)
    {
        var questionOption = await questionOptions.Where(qst => qst.Id == command.QuestionOptionId
                                                             && qst.Question!.RegistrationForm!.EventId == command.EventId)
                                                  .Include(qst => qst.Mappings!)
                                                  .ThenInclude(trk => trk.Registrable)
                                                  .Include(qst => qst.Responses!)
                                                  .ThenInclude(rsp => rsp.Registration!.Seats_AsLeader)
                                                  .FirstAsync(cancellationToken);

        foreach (var mapping in questionOption.Mappings!)
        {
            if (mapping.Registrable is { } registrable)
            {
                var hasWaitingList = registrable.MaximumSingleSeats != null;
                if (registrable.Type == RegistrableType.Single)
                {
                    foreach (var response in questionOption.Responses!.Where(rsp=>rsp.Registration!.State != RegistrationState.Cancelled))
                    {
                        var spotExists = response.Registration?.Seats_AsLeader?.Any(spt => spt.RegistrableId == registrable.Id) == true;
                        if (!spotExists)
                        {
                            spots.InsertObjectTree(new Seat
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       FirstPartnerJoined = response.Registration!.ReceivedAt,
                                                       RegistrationId = response.RegistrationId,
                                                       RegistrableId = registrable.Id,
                                                       IsWaitingList = hasWaitingList
                                                   });
                            changeTrigger.TriggerUpdate<RegistrationCalculator>(response.RegistrationId, command.EventId);
                        }
                    }

                    if (hasWaitingList)
                    {
                        changeTrigger.EnqueueCommand(new TriggerMoveUpFromWaitingListCommand
                                                     {
                                                         EventId = command.EventId,
                                                         RegistrableId = registrable.Id
                                                     });
                    }
                    else
                    {
                        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
                        changeTrigger.QueryChanged<ParticipantsOfRegistrableQuery>(command.EventId, registrable.Id);
                    }
                }
                else if (registrable.Type == RegistrableType.Double)
                {
                    // Todo
                }
            }
            else
            {
                // Todo: reprocessing of types like name, email, phone, ...
            }
        }
    }
}