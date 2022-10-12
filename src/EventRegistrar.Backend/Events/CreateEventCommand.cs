using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Compositions;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Events;

public class CreateEventCommand : IRequest
{
    public string Acronym { get; set; }
    public Guid? EventId_Predecessor { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool CopyAccessRights { get; set; }
    public bool CopyRegistrables { get; set; }
    public bool CopyAutoMailTemplates { get; set; }
    public bool CopyConfigurations { get; set; }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand>
{
    private readonly AuthenticatedUserId _authenticatedUserId;
    private readonly CommandQueue _commandQueue;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRepository<Event> _events;
    private readonly IRepository<Reduction> _reductions;
    private readonly IRepository<RegistrableComposition> _registrableCompositions;
    private readonly IEventBus _eventBus;

    public CreateEventCommandHandler(IRepository<Event> events,
                                     IRepository<Reduction> reductions,
                                     IRepository<RegistrableComposition> registrableCompositions,
                                     AuthenticatedUserId authenticatedUserId,
                                     CommandQueue commandQueue,
                                     IDateTimeProvider dateTimeProvider,
                                     IEventBus eventBus)
    {
        _events = events;
        _reductions = reductions;
        _registrableCompositions = registrableCompositions;
        _authenticatedUserId = authenticatedUserId;
        _commandQueue = commandQueue;
        _dateTimeProvider = dateTimeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        var existingEvent = await _events.FirstOrDefaultAsync(evt => evt.Acronym == command.Acronym
                                                                  || evt.Id == command.Id, cancellationToken);
        if (existingEvent != null)
        {
            throw new Exception($"Event with Acronym {existingEvent.Acronym} already exists (Id {existingEvent.Id})");
        }

        if (_authenticatedUserId.UserId == null)
        {
            throw new UnauthorizedAccessException("Unknown authenticated user");
        }

        // create event
        var newEventId = command.Id;
        var newEvent = new Event
                       {
                           Id = newEventId,
                           Acronym = command.Acronym,
                           State = EventState.Setup,
                           Name = command.Name,
                           PredecessorEventId = command.EventId_Predecessor,
                           Users = new List<UserInEvent>(new[]
                                                         {
                                                             // make creator admin
                                                             new UserInEvent
                                                             {
                                                                 Id = newEventId,
                                                                 Role = UserInEventRole.Admin,
                                                                 UserId = _authenticatedUserId.UserId!.Value
                                                             }
                                                         }),
                           Registrables = new List<Registrable>()
                       };

        // copy from other event?
        if (command.EventId_Predecessor != null)
        {
            var sourceEventId = command.EventId_Predecessor.Value;
            var sourceEvent = await _events.Where(evt => evt.Id == sourceEventId)
                                           .Include(evt => evt.Users)
                                           .Include(evt => evt.Configurations)
                                           .Include(evt => evt.Registrables!)
                                           .ThenInclude(rbl => rbl.Reductions)
                                           .Include(evt => evt.Registrables!)
                                           .ThenInclude(rbl => rbl.Compositions)
                                           .Include(evt => evt.AutoMailTemplates)
                                           .FirstAsync(cancellationToken);

            var isUserAdminInOtherEvent = sourceEvent.Users!.Any(uie => uie.UserId == _authenticatedUserId.UserId);

            if (!isUserAdminInOtherEvent)
            {
                throw new Exception($"You are not admin of event {sourceEvent.Name} / {sourceEventId}. Only an admin can copy an event");
            }

            if (command.CopyAccessRights)
            {
                sourceEvent.Users!
                           .Where(uie => uie.UserId != _authenticatedUserId.UserId)
                           .ForEach(uie => newEvent.Users.Add(new UserInEvent
                                                              {
                                                                  Id = Guid.NewGuid(),
                                                                  EventId = newEventId,
                                                                  Role = uie.Role,
                                                                  UserId = uie.UserId
                                                              }));
            }

            if (command.CopyAutoMailTemplates)
            {
                newEvent.AutoMailTemplates = sourceEvent.AutoMailTemplates!
                                                        .Select(amt => new AutoMailTemplate
                                                                       {
                                                                           Id = Guid.NewGuid(),
                                                                           Type = amt.Type,
                                                                           Language = amt.Language,
                                                                           Subject = amt.Subject,
                                                                           ContentHtml = amt.ContentHtml,
                                                                           ReleaseImmediately = amt.ReleaseImmediately
                                                                       })
                                                        .ToList();
            }

            if (command.CopyRegistrables)
            {
                // ToDo: map RegistrableId1_ReductionActivatedIfCombinedWith etc to new ids
                var registrableMap = new Dictionary<Guid, Guid>();
                foreach (var sourceRegistrable in sourceEvent.Registrables!)
                {
                    var newRegistrableId = Guid.NewGuid();
                    newEvent.Registrables!.Add(new Registrable
                                               {
                                                   Id = newRegistrableId,
                                                   EventId = newEventId,
                                                   Price = sourceRegistrable.Price,
                                                   MaximumDoubleSeats = sourceRegistrable.MaximumDoubleSeats,
                                                   MaximumSingleSeats = sourceRegistrable.MaximumSingleSeats,
                                                   MaximumAllowedImbalance = sourceRegistrable.MaximumAllowedImbalance,
                                                   Name = sourceRegistrable.Name,
                                                   CheckinListColumn = sourceRegistrable.CheckinListColumn,
                                                   HasWaitingList = sourceRegistrable.HasWaitingList,
                                                   ShowInMailListOrder = sourceRegistrable.ShowInMailListOrder,
                                                   IsCore = sourceRegistrable.IsCore
                                               });

                    registrableMap.Add(sourceRegistrable.Id, newRegistrableId);
                }

                foreach (var registrableOfSourceEvent in sourceEvent.Registrables!)
                {
                    var newRegistrableId = registrableMap[registrableOfSourceEvent.Id];
                    // copy reductions
                    foreach (var reduction in registrableOfSourceEvent.Reductions!)
                    {
                        _reductions.InsertObjectTree(new Reduction
                                                     {
                                                         Id = Guid.NewGuid(),
                                                         RegistrableId = newRegistrableId,
                                                         Amount = reduction.Amount,
                                                         OnlyForRole = reduction.OnlyForRole,
                                                         //QuestionOptionId_ActivatesReduction = reduction.QuestionOptionId_ActivatesReduction,
                                                         RegistrableId1_ReductionActivatedIfCombinedWith =
                                                             registrableMap.Lookup(reduction.RegistrableId1_ReductionActivatedIfCombinedWith),
                                                         RegistrableId2_ReductionActivatedIfCombinedWith =
                                                             registrableMap.Lookup(reduction.RegistrableId2_ReductionActivatedIfCombinedWith)
                                                     });
                    }

                    // copy compositions
                    foreach (var composition in registrableOfSourceEvent.Compositions!)
                    {
                        var mappedRegistrableId_Contains = registrableMap.Lookup(composition.RegistrableId_Contains);
                        if (mappedRegistrableId_Contains != null)
                        {
                            _registrableCompositions.InsertObjectTree(new RegistrableComposition
                                                                      {
                                                                          Id = Guid.NewGuid(),
                                                                          RegistrableId = newRegistrableId,
                                                                          RegistrableId_Contains = mappedRegistrableId_Contains.Value
                                                                      });
                        }
                    }
                }
            }

            if (command.CopyConfigurations)
            {
                newEvent.Configurations = sourceEvent.Configurations!
                                                     .Select(cfg => new EventConfiguration
                                                                    {
                                                                        Id = Guid.NewGuid(),
                                                                        Type = cfg.Type,
                                                                        ValueJson = cfg.ValueJson
                                                                    })
                                                     .ToList();
            }
        }

        _events.InsertObjectTree(newEvent);
        var now = _dateTimeProvider.Now;
        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         EventId = newEventId,
                                         DirtyMoment = now,
                                         QueryName = nameof(RegistrablesOverviewQuery)
                                     });
        _commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                     {
                                         EventId = newEventId,
                                         DirtyMoment = now,
                                         QueryName = nameof(DuePaymentsQuery)
                                     });
        _eventBus.Publish(new ReadModelUpdated { QueryName = nameof(EventsOfUserQuery) });

        return Unit.Value;
    }
}