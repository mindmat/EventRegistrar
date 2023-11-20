namespace EventRegistrar.Backend.PhoneMessages;

public class SmsConversationQuery : IRequest<IEnumerable<SmsDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SmsConversationQueryHandler(IQueryable<Sms> sms) : IRequestHandler<SmsConversationQuery, IEnumerable<SmsDisplayItem>>
{
    public async Task<IEnumerable<SmsDisplayItem>> Handle(SmsConversationQuery query,
                                                          CancellationToken cancellationToken)
    {
        return await sms.Where(s => s.RegistrationId == query.RegistrationId)
                        .Select(s => new SmsDisplayItem
                                     {
                                         Status = s.SmsStatus,
                                         Body = s.Body,
                                         Sent = s.Sent != null,
                                         Date = s.Sent ?? s.Received
                                     })
                        .OrderBy(s => s.Date)
                        .ToListAsync(cancellationToken);
    }
}