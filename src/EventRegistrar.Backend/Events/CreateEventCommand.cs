using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Compositions;
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
    public bool CopyBulkMailTemplates { get; set; }
    public bool CopyConfigurations { get; set; }
}

public class CreateEventCommandHandler(IRepository<Event> events,
                                       IRepository<RegistrableComposition> registrableCompositions,
                                       AuthenticatedUserId authenticatedUserId,
                                       AuthenticatedUser authenticatedUser,
                                       ChangeTrigger changeTrigger,
                                       IEventBus eventBus,
                                       IRepository<User> users)
    : IRequestHandler<CreateEventCommand>
{
    public async Task Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Name must not be empty", nameof(command.Name));
        }

        if (string.IsNullOrWhiteSpace(command.Acronym))
        {
            throw new ArgumentException("Acronym must not be empty", nameof(command.Acronym));
        }

        var existingEvent = await events.FirstOrDefaultAsync(evt => evt.Acronym == command.Acronym
                                                                 || evt.Id == command.Id, cancellationToken);
        if (existingEvent != null)
        {
            throw new Exception($"Event with Acronym {existingEvent.Acronym} already exists (Id {existingEvent.Id})");
        }

        var userId = CreateUserIfNecessary();

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
                                                                 UserId = userId
                                                             }
                                                         }),
                           Registrables = new List<Registrable>()
                       };

        // copy from other event?
        if (command.EventId_Predecessor != null)
        {
            var sourceEventId = command.EventId_Predecessor.Value;
            var sourceEvent = await events.Where(evt => evt.Id == sourceEventId)
                                          .Include(evt => evt.Users)
                                          .Include(evt => evt.Configurations)
                                          .Include(evt => evt.Registrables!)
                                          .ThenInclude(rbl => rbl.Reductions)
                                          .Include(evt => evt.Registrables!)
                                          .ThenInclude(rbl => rbl.Compositions)
                                          .Include(evt => evt.AutoMailTemplates)
                                          .Include(evt => evt.BulkMailTemplates)
                                          .FirstAsync(cancellationToken);

            var isUserAdminInOtherEvent = sourceEvent.Users!.Any(uie => uie.UserId == userId);

            if (!isUserAdminInOtherEvent)
            {
                throw new Exception($"You are not admin of event {sourceEvent.Name} / {sourceEventId}. Only an admin can copy an event");
            }

            if (command.CopyAccessRights)
            {
                sourceEvent.Users!
                           .Where(uie => uie.UserId != userId)
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

            if (command.CopyBulkMailTemplates)
            {
                newEvent.BulkMailTemplates = sourceEvent.BulkMailTemplates!
                                                        .Select(bmt => new BulkMailTemplate
                                                                       {
                                                                           Id = Guid.NewGuid(),
                                                                           BulkMailKey = bmt.BulkMailKey,
                                                                           Language = bmt.Language,
                                                                           Subject = bmt.Subject,
                                                                           ContentHtml = bmt.ContentHtml,
                                                                           MailingAudience = bmt.MailingAudience,
                                                                           RegistrableId = bmt.RegistrableId
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
                                                   NameSecondary = sourceRegistrable.NameSecondary,
                                                   DisplayName = sourceRegistrable.DisplayName,
                                                   CheckinListColumn = sourceRegistrable.CheckinListColumn,
                                                   HasWaitingList = sourceRegistrable.HasWaitingList,
                                                   ShowInMailListOrder = sourceRegistrable.ShowInMailListOrder,
                                                   IsCore = sourceRegistrable.IsCore,
                                                   Type = sourceRegistrable.Type,
                                                   AutomaticPromotionFromWaitingList = sourceRegistrable.AutomaticPromotionFromWaitingList,
                                                   Tag = sourceRegistrable.Tag
                                               });

                    registrableMap.Add(sourceRegistrable.Id, newRegistrableId);
                }

                foreach (var registrableOfSourceEvent in sourceEvent.Registrables!)
                {
                    var newRegistrableId = registrableMap[registrableOfSourceEvent.Id];

                    // copy compositions
                    foreach (var composition in registrableOfSourceEvent.Compositions!)
                    {
                        var mappedRegistrableId_Contains = registrableMap.Lookup(composition.RegistrableId_Contains);
                        if (mappedRegistrableId_Contains != null)
                        {
                            registrableCompositions.InsertObjectTree(new RegistrableComposition
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

        events.InsertObjectTree(newEvent);

        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, newEventId);
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, newEventId);

        eventBus.Publish(new QueryChanged { QueryName = nameof(EventsOfUserQuery) });
        eventBus.Publish(new QueryChanged { QueryName = nameof(SearchEventQuery) });
    }

    private Guid CreateUserIfNecessary()
    {
        if (authenticatedUserId.UserId != null)
        {
            return authenticatedUserId.UserId.Value;
        }

        if (authenticatedUser == AuthenticatedUser.None)
        {
            throw new UnauthorizedAccessException("Unknown authenticated user");
        }

        var user = new User
                   {
                       Id = Guid.NewGuid(),
                       IdentityProvider = authenticatedUser.IdentityProvider,
                       IdentityProviderUserIdentifier = authenticatedUser.IdentityProviderUserIdentifier,
                       FirstName = authenticatedUser.FirstName,
                       LastName = authenticatedUser.LastName,
                       Email = authenticatedUser.Email
                   };
        users.InsertObjectTree(user);

        return user.Id;
    }
}