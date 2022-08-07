using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Spots;

using MediatR;

namespace EventRegistrar.Backend.Registrations.ReadModels;

public class UpdateRegistrationQueryReadModelCommand : IRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class UpdateRegistrationQueryReadModelCommandHandler : IRequestHandler<UpdateRegistrationQueryReadModelCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Seat> _spots;
    private readonly IQueryable<PaymentAssignment> _assignments;
    private readonly IEventBus _eventBus;

    private readonly DbContext _dbContext;
    private readonly EnumTranslator _enumTranslator;

    public UpdateRegistrationQueryReadModelCommandHandler(IQueryable<Registration> registrations,
                                                          IQueryable<Seat> spots,
                                                          DbContext dbContext,
                                                          EnumTranslator enumTranslator,
                                                          IQueryable<PaymentAssignment> assignments,
                                                          IEventBus eventBus)
    {
        _registrations = registrations;
        _spots = spots;
        _dbContext = dbContext;
        _enumTranslator = enumTranslator;
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateRegistrationQueryReadModelCommand command, CancellationToken cancellationToken)
    {
        var content = await _registrations.Where(reg => reg.EventId == command.EventId
                                                     && reg.Id == command.RegistrationId)
                                          .Select(reg => new RegistrationDisplayItem
                                                         {
                                                             Id = reg.Id,
                                                             IsWaitingList = reg.IsWaitingList,
                                                             Price = reg.Price,
                                                             Status = reg.State,
                                                             StatusText = reg.State.ToString(),
                                                             Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                                     ? asn.Amount
                                                                                                                     : -asn.Amount)
                                                                 ?? 0m,
                                                             Language = reg.Language,
                                                             ReceivedAt = reg.ReceivedAt,
                                                             ReminderLevel = reg.ReminderLevel,
                                                             Remarks = reg.Remarks,
                                                             Email = reg.RespondentEmail,
                                                             FirstName = reg.RespondentFirstName,
                                                             LastName = reg.RespondentLastName,
                                                             SoldOutMessage = reg.SoldOutMessage,
                                                             FallbackToPartyPass = reg.FallbackToPartyPass,
                                                             SmsCount = reg.Sms!.Count,
                                                             PhoneNormalized = reg.PhoneNormalized,
                                                             PhoneFormatted = reg.PhoneNormalized,
                                                             PartnerOriginal = reg.PartnerNormalized == null
                                                                                   ? null
                                                                                   : reg.PartnerOriginal,
                                                             PartnerName = reg.RegistrationId_Partner == null
                                                                               ? null
                                                                               : $"{reg.Registration_Partner!.RespondentFirstName} {reg.Registration_Partner.RespondentLastName}",
                                                             PartnerId = reg.RegistrationId_Partner,
                                                             IsReduced = reg.IsReduced,
                                                             WillPayAtCheckin = reg.WillPayAtCheckin
                                                         })
                                          .FirstAsync(cancellationToken);

        content.Spots = await _spots.Where(spot => (spot.Registration!.EventId == command.EventId
                                                 || spot.Registration_Follower!.EventId == command.EventId)
                                                && (spot.RegistrationId == command.RegistrationId
                                                 || spot.RegistrationId_Follower == command.RegistrationId))
                                    .Where(spot => !spot.IsCancelled)
                                    .OrderByDescending(spot => spot.Registrable!.IsCore)
                                    .ThenBy(spot => spot.Registrable!.ShowInMailListOrder)
                                    .Select(spot => new SpotDisplayItem
                                                    {
                                                        Id = spot.Id,
                                                        RegistrableId = spot.RegistrableId,
                                                        RegistrableName = spot.Registrable!.Name,
                                                        RegistrableNameSecondary = spot.Registrable.NameSecondary,
                                                        PartnerRegistrationId = spot.IsPartnerSpot
                                                                                    ? spot.RegistrationId == command.RegistrationId
                                                                                          ? spot.RegistrationId_Follower
                                                                                          : spot.RegistrationId
                                                                                    : null,
                                                        FirstPartnerJoined = spot.FirstPartnerJoined,
                                                        IsCore = spot.Registrable.IsCore,
                                                        RoleText = spot.Registrable.Type == RegistrableType.Double
                                                                       ? _enumTranslator.Translate(spot.RegistrationId == command.RegistrationId
                                                                                                       ? Role.Leader
                                                                                                       : Role.Follower)
                                                                       : null,
                                                        PartnerName = spot.IsPartnerSpot
                                                                          ? spot.RegistrationId == command.RegistrationId
                                                                                ? $"{spot.Registration_Follower!.RespondentFirstName} {spot.Registration_Follower.RespondentLastName}"
                                                                                : $"{spot.Registration!.RespondentFirstName} {spot.Registration.RespondentLastName}"
                                                                          : null,
                                                        IsWaitingList = spot.IsWaitingList,
                                                        Type = spot.Registrable.Type
                                                    })
                                    .ToListAsync(cancellationToken);

        var data = await _assignments.Where(ass => ass.Registration!.EventId == command.EventId
                                                && ass.RegistrationId == command.RegistrationId
                                                && ass.PaymentAssignmentId_Counter == null)
                                     .Select(ass => new
                                                    {
                                                        ass.Id,
                                                        ass.Amount,
                                                        ass.IncomingPaymentId,
                                                        ass.OutgoingPaymentId,
                                                        Currency_Incoming = ass.IncomingPayment!.Payment!.Currency,
                                                        BookingDate_Incoming = (DateTime?)ass.IncomingPayment.Payment.BookingDate,
                                                        Currency_Outgoing = ass.OutgoingPayment!.Payment!.Currency,
                                                        BookingDate_Outgoing = (DateTime?)ass.OutgoingPayment.Payment.BookingDate
                                                    })
                                     .ToListAsync(cancellationToken);

        content.Payments = data.Where(ass => ass.IncomingPaymentId != null)
                               .Select(ass => new AssignedPaymentDisplayItem
                                              {
                                                  PaymentAssignmentId = ass.Id,
                                                  Amount = ass.Amount,
                                                  Currency = ass.Currency_Incoming,
                                                  BookingDate = ass.BookingDate_Incoming!.Value
                                              })
                               .Concat(data.Where(ass => ass.OutgoingPaymentId != null)
                                           .Select(ass => new AssignedPaymentDisplayItem
                                                          {
                                                              PaymentAssignmentId = ass.Id,
                                                              Amount = -ass.Amount,
                                                              Currency = ass.Currency_Outgoing,
                                                              BookingDate = ass.BookingDate_Outgoing!.Value
                                                          }))
                               .ToList();

        var readModels = _dbContext.Set<RegistrationQueryReadModel>();

        var readModel = await readModels.AsTracking()
                                        .Where(rm => rm.EventId == command.EventId
                                                  && rm.RegistrationId == command.RegistrationId)
                                        .FirstOrDefaultAsync(cancellationToken);
        if (readModel == null)
        {
            readModel = new RegistrationQueryReadModel
                        {
                            EventId = command.EventId,
                            RegistrationId = command.RegistrationId,
                            Content = content
                        };
            var entry = readModels.Attach(readModel);
            entry.State = EntityState.Added;
            _eventBus.Publish(new ReadModelUpdated
                              {
                                  EventId = command.EventId,
                                  QueryName = nameof(RegistrationQueryReadModel),
                                  RowId = command.RegistrationId
                              });
        }
        else
        {
            readModel.Content = content;
            if (_dbContext.Entry(readModel).State == EntityState.Modified)
            {
                _eventBus.Publish(new ReadModelUpdated
                                  {
                                      EventId = command.EventId,
                                      QueryName = nameof(RegistrationQueryReadModel),
                                      RowId = command.RegistrationId
                                  });
            }
        }

        return Unit.Value;
    }
}

public class UpdateRegistrationWhenOutgoingPaymentAssigned : IEventToCommandTranslation<OutgoingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}

public class UpdateRegistrationWhenOutgoingPaymentUnassigned : IEventToCommandTranslation<OutgoingPaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(OutgoingPaymentUnassigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}

public class UpdateRegistrationWhenIncomingPaymentUnassigned : IEventToCommandTranslation<IncomingPaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}

public class UpdateRegistrationWhenIncomingPaymentAssigned : IEventToCommandTranslation<IncomingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        if (e.EventId != null && e.RegistrationId != null)
        {
            yield return new UpdateRegistrationQueryReadModelCommand
                         {
                             EventId = e.EventId.Value,
                             RegistrationId = e.RegistrationId.Value
                         };
        }
    }
}