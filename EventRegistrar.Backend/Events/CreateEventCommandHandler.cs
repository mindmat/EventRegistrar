using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Compositions;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.RegistrationForms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand>
    {
        private readonly AuthenticatedUserId _authenticatedUserId;
        private readonly IRepository<EventConfiguration> _configurations;
        private readonly IRepository<Event> _events;
        private readonly IRepository<MailTemplate> _mailTemplates;
        private readonly IRepository<Reduction> _reductions;
        private readonly IRepository<RegistrableComposition> _registrableCompositions;
        private readonly IRepository<Registrable> _registrables;
        private readonly IRepository<UserInEvent> _usersInEvents;

        public CreateEventCommandHandler(IRepository<Event> events,
                                         IRepository<UserInEvent> usersInEvents,
                                         IRepository<MailTemplate> mailTemplates,
                                         IRepository<Registrable> registrables,
                                         IRepository<Reduction> reductions,
                                         IRepository<RegistrableComposition> registrableCompositions,
                                         IRepository<EventConfiguration> configurations,
                                         AuthenticatedUserId authenticatedUserId)
        {
            _events = events;
            _usersInEvents = usersInEvents;
            _registrables = registrables;
            _reductions = reductions;
            _registrableCompositions = registrableCompositions;
            _configurations = configurations;
            _mailTemplates = mailTemplates;
            _authenticatedUserId = authenticatedUserId;
        }

        public async Task<Unit> Handle(CreateEventCommand command, CancellationToken cancellationToken)
        {
            var existingEvent = await _events.FirstOrDefaultAsync(evt => evt.Acronym == command.Acronym
                                                                      || evt.Id == command.Id, cancellationToken);
            if (existingEvent != null)
            {
                throw new Exception($"Event with Acronym {existingEvent.Acronym} already exists (Id {existingEvent.Id})");
            }

            if (!_authenticatedUserId.UserId.HasValue)
            {
                throw new UnauthorizedAccessException("Unknown authenticated user");
            }

            // create event
            var newEventId = command.Id;
            var newEvent = new Event
            {
                Id = newEventId,
                Acronym = command.Acronym,
                State = State.Setup,
                Name = command.Name
            };
            await _events.InsertOrUpdateEntity(newEvent, cancellationToken);

            // make creator admin
            await _usersInEvents.InsertOrUpdateEntity(new UserInEvent
            {
                Id = newEventId,
                EventId = newEvent.Id,
                Role = UserInEventRole.Admin,
                UserId = _authenticatedUserId.UserId.Value
            }, cancellationToken);

            // copy from other event?
            if (command.EventId_CopyFrom.HasValue)
            {
                var sourceEventId = command.EventId_CopyFrom.Value;
                var accessRightsInSourceEvent = await _usersInEvents.Where(uie => uie.EventId == sourceEventId)
                                                                    .ToListAsync(cancellationToken);
                var isUserAdminInOtherEvent = accessRightsInSourceEvent.Any(uie => uie.UserId == _authenticatedUserId.UserId.Value);

                if (!isUserAdminInOtherEvent)
                {
                    throw new Exception($"You are not admin of event {sourceEventId}. Only an admin can copy an event");
                }

                // copy access rights
                foreach (var accessRightInSourceEvent in accessRightsInSourceEvent.Where(uie => uie.UserId != _authenticatedUserId.UserId.Value))
                {
                    await _usersInEvents.InsertOrUpdateEntity(new UserInEvent
                    {
                        Id = Guid.NewGuid(),
                        EventId = newEventId,
                        Role = accessRightInSourceEvent.Role,
                        UserId = accessRightInSourceEvent.UserId
                    }, cancellationToken);
                }

                // copy mail templates
                var mailTemplatesOfSourceEvent = await _mailTemplates.Where(mtp => mtp.EventId == sourceEventId
                                                                                && !mtp.IsDeleted)
                                                                     .ToListAsync(cancellationToken);
                foreach (var mailTemplateOfSourceEvent in mailTemplatesOfSourceEvent)
                {
                    await _mailTemplates.InsertOrUpdateEntity(new MailTemplate
                    {
                        Id = Guid.NewGuid(),
                        EventId = newEventId,
                        Template = mailTemplateOfSourceEvent.Template,
                        Language = mailTemplateOfSourceEvent.Language,
                        Type = mailTemplateOfSourceEvent.Type,
                        BulkMailKey = mailTemplateOfSourceEvent.BulkMailKey,
                        MailingAudience = mailTemplateOfSourceEvent.MailingAudience,
                        ContentType = mailTemplateOfSourceEvent.ContentType,
                        SenderMail = mailTemplateOfSourceEvent.SenderMail,
                        SenderName = mailTemplateOfSourceEvent.SenderName,
                        Subject = mailTemplateOfSourceEvent.Subject
                    }, cancellationToken);
                }

                // copy registrables
                // ToDo: map RegistrableId1_ReductionActivatedIfCombinedWith etc to new ids
                var registrablesOfSourceEvent = await _registrables.Where(mtp => mtp.EventId == sourceEventId)
                                                                   .Include(mtp => mtp.Reductions)
                                                                   .Include(mtp => mtp.Compositions)
                                                                   .ToListAsync(cancellationToken);
                foreach (var registrableOfSourceEvent in registrablesOfSourceEvent)
                {
                    var newRegistrableId = Guid.NewGuid();
                    await _registrables.InsertOrUpdateEntity(new Registrable
                    {
                        Id = newRegistrableId,
                        EventId = newEventId,
                        Price = registrableOfSourceEvent.Price,
                        MaximumDoubleSeats = registrableOfSourceEvent.MaximumDoubleSeats,
                        MaximumSingleSeats = registrableOfSourceEvent.MaximumSingleSeats,
                        MaximumAllowedImbalance = registrableOfSourceEvent.MaximumAllowedImbalance,
                        Name = registrableOfSourceEvent.Name,
                        CheckinListColumn = registrableOfSourceEvent.CheckinListColumn,
                        HasWaitingList = registrableOfSourceEvent.HasWaitingList,
                        ShowInMailListOrder = registrableOfSourceEvent.ShowInMailListOrder,
                        IsCore = registrableOfSourceEvent.IsCore
                    }, cancellationToken);

                    // copy reductions
                    foreach (var reduction in registrableOfSourceEvent.Reductions.ToList())
                    {
                        await _reductions.InsertOrUpdateEntity(new Reduction
                        {
                            Id = Guid.NewGuid(),
                            RegistrableId = newRegistrableId,
                            Amount = reduction.Amount,
                            OnlyForRole = reduction.OnlyForRole,
                            QuestionOptionId_ActivatesReduction = reduction.QuestionOptionId_ActivatesReduction,
                            RegistrableId1_ReductionActivatedIfCombinedWith = reduction.RegistrableId1_ReductionActivatedIfCombinedWith,
                            RegistrableId2_ReductionActivatedIfCombinedWith = reduction.RegistrableId2_ReductionActivatedIfCombinedWith
                        }, cancellationToken);
                    }

                    // copy compositions
                    foreach (var composition in registrableOfSourceEvent.Compositions)
                    {
                        await _registrableCompositions.InsertOrUpdateEntity(new RegistrableComposition
                        {
                            Id = Guid.NewGuid(),
                            RegistrableId = newRegistrableId,
                            RegistrableId_Contains = composition.RegistrableId_Contains
                        }, cancellationToken);
                    }
                }

                // copy configs
                var configurationsOfSourceEvent = await _configurations.Where(mtp => mtp.EventId == sourceEventId)
                                                                       .ToListAsync(cancellationToken);
                foreach (var configurationOfSourceEvent in configurationsOfSourceEvent)
                {
                    await _configurations.InsertOrUpdateEntity(new EventConfiguration
                    {
                        Id = Guid.NewGuid(),
                        EventId = newEventId,
                        Type = configurationOfSourceEvent.Type,
                        ValueJson = configurationOfSourceEvent.ValueJson
                    }, cancellationToken);
                }
            }
            return Unit.Value;
        }
    }
}