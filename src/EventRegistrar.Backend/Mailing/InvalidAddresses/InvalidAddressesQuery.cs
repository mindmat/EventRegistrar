using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;
using MediatR;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class InvalidAddress
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public DateTime? LastMailSent { get; set; }
    public string LastMailState { get; set; }
    public string LastName { get; set; }
    public string Proposals { get; set; }
    public Guid RegistrationId { get; set; }
    public string RegistrationState { get; set; }
}

public class InvalidAddressesQuery : IRequest<IEnumerable<InvalidAddress>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class InvalidAddressesQueryHandler : IRequestHandler<InvalidAddressesQuery, IEnumerable<InvalidAddress>>
{
    private static readonly MailState?[] DeliveredMailStates = { MailState.Delivered, MailState.Open, MailState.Click };
    private readonly IQueryable<Registration> _registrations;

    public InvalidAddressesQueryHandler(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<IEnumerable<InvalidAddress>> Handle(InvalidAddressesQuery query,
                                                          CancellationToken cancellationToken)
    {
        var invalidMails = (await _registrations.Where(reg => reg.EventId == query.EventId
                                                           && reg.State != RegistrationState.Cancelled
                                                           && reg.Mails.Any(map => map.State != null)
                                                           && !reg.Mails.Any(map =>
                                                                  map.State != null &&
                                                                  DeliveredMailStates.Contains(map.State)))
                                                .Select(reg => new
                                                               {
                                                                   RegistrationId = reg.Id,
                                                                   RegistrationState = reg.State,
                                                                   FirstName = reg.RespondentFirstName,
                                                                   LastName = reg.RespondentLastName,
                                                                   Email = reg.RespondentEmail,
                                                                   LastMailSent = reg.Mails
                                                                       .Where(map => map.State != null)
                                                                       .OrderByDescending(map => map.Mail.Sent)
                                                                       .Select(map => new { map.Mail.Sent, map.State })
                                                                       .First()
                                                               })
                                                .ToListAsync(cancellationToken)
                           )
                           .Select(reg => new InvalidAddress
                                          {
                                              RegistrationId = reg.RegistrationId,
                                              RegistrationState = reg.RegistrationState.ToString(),
                                              FirstName = reg.FirstName,
                                              LastName = reg.LastName,
                                              Email = reg.Email,
                                              LastMailSent = reg.LastMailSent?.Sent,
                                              LastMailState = reg.LastMailSent?.Sent.ToString()
                                          })
                           .OrderByDescending(mail => mail.LastMailSent)
                           .ToList();
        foreach (var invalidMail in invalidMails)
        {
            var otherRegistrations = await _registrations.Where(reg => reg.Id != invalidMail.RegistrationId
                                                                    && reg.RespondentFirstName == invalidMail.FirstName
                                                                    && reg.RespondentLastName == invalidMail.LastName
                                                                    && reg.Mails.Any(map =>
                                                                           (map.State ?? map.Mail.State) == null ||
                                                                           DeliveredMailStates.Contains(
                                                                               (map.State ?? map.Mail.State).Value)))
                                                         .Select(reg => reg.RespondentEmail)
                                                         .ToListAsync(cancellationToken);
            invalidMail.Proposals = otherRegistrations.Distinct().StringJoin(";");
        }

        return invalidMails;
    }
}