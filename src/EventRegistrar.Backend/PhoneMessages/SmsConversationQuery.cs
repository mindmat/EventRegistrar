using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.PhoneMessages;

public class SmsConversationQuery : IRequest<IEnumerable<SmsDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SmsConversationQueryHandler : IRequestHandler<SmsConversationQuery, IEnumerable<SmsDisplayItem>>
{
    private readonly IQueryable<Sms> _sms;

    public SmsConversationQueryHandler(IQueryable<Sms> sms)
    {
        _sms = sms;
    }

    public async Task<IEnumerable<SmsDisplayItem>> Handle(SmsConversationQuery query,
                                                          CancellationToken cancellationToken)
    {
        return await _sms.Where(s => s.RegistrationId == query.RegistrationId)
                         .Select(s => new SmsDisplayItem
                                      {
                                          Status = s.SmsStatus,
                                          Body = s.Body,
                                          Sent = s.Sent.HasValue,
                                          Date = s.Sent ?? s.Received
                                      })
                         .OrderBy(s => s.Date)
                         .ToListAsync(cancellationToken);
    }
}