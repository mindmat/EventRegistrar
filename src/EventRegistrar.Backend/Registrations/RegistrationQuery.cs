using EventRegistrar.Backend.Authorization;

using MediatR;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationQuery : IRequest<RegistrationDisplayItem?>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RegistrationQueryHandler : IRequestHandler<RegistrationQuery, RegistrationDisplayItem?>
{
    private readonly IQueryable<Registration> _registrations;

    public RegistrationQueryHandler(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<RegistrationDisplayItem?> Handle(RegistrationQuery query, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.EventId == query.EventId
                                                          && reg.Id == query.RegistrationId)
                                               .Select(reg => new RegistrationDisplayItem
                                                              {
                                                                  Id = reg.Id,
                                                                  IsWaitingList = reg.IsWaitingList,
                                                                  Price = reg.Price,
                                                                  Status = reg.State,
                                                                  StatusText = reg.State.ToString(),
                                                                  Paid = (decimal?)reg.Payments!.Sum(asn =>
                                                                                                         asn.PayoutRequestId == null
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
                                               .FirstOrDefaultAsync(cancellationToken);
        return registration;
    }
}