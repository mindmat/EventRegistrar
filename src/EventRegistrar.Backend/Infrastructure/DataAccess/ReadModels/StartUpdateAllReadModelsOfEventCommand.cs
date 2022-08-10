using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Assignments.Candidates;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrables.ReadModels;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class StartUpdateAllReadModelsOfEventCommand : IRequest
{
    public Guid EventId { get; set; }
}

public class StartUpdateAllReadModelsOfEventCommandHandler : IRequestHandler<StartUpdateAllReadModelsOfEventCommand>
{
    private readonly CommandQueue _commandQueue;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Payment> _payments;

    public StartUpdateAllReadModelsOfEventCommandHandler(CommandQueue commandQueue,
                                                         IQueryable<Registration> registrations,
                                                         IQueryable<Payment> payments)
    {
        _commandQueue = commandQueue;
        _registrations = registrations;
        _payments = payments;
    }

    public async Task<Unit> Handle(StartUpdateAllReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        _commandQueue.EnqueueCommand(new UpdateRegistrablesOverviewQueryReadModelCommand { EventId = command.EventId });

        var registrationIds = await _registrations.Where(reg => reg.EventId == command.EventId)
                                                  .Select(reg => reg.Id)
                                                  .ToListAsync(cancellationToken);

        foreach (var registrationId in registrationIds)
        {
            _commandQueue.EnqueueCommand(new UpdateRegistrationReadModelCommand
                                         {
                                             EventId = command.EventId,
                                             RegistrationId = registrationId
                                         });
        }

        var paymentIds = await _payments.Where(pmt => pmt.PaymentsFile!.EventId == command.EventId)
                                        .Select(pmt => pmt.Id)
                                        .ToListAsync(cancellationToken);

        foreach (var paymentId in paymentIds)
        {
            _commandQueue.EnqueueCommand(new UpdatePaymentAssignmentsCommand
                                         {
                                             EventId = command.EventId,
                                             PaymentId = paymentId
                                         });
        }


        return Unit.Value;
    }
}