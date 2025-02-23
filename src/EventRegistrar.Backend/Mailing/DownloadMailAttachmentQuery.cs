
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Mailing;

public class DownloadMailAttachmentQuery : IRequest<DownloadResult>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailAttachmentId { get; set; }
}
    
public class DownloadMailAttachmentQueryHandler(IQueryable<MailAttachment> _attachments) : IRequestHandler<DownloadMailAttachmentQuery, DownloadResult>
{
    public async Task<DownloadResult> Handle(DownloadMailAttachmentQuery query, CancellationToken cancellationToken)
    {
        var attachment = await _attachments.Where(att => att.Mail!.EventId == query.EventId
                                                      && att.Id == query.MailAttachmentId)
                                           .Select(att => new DownloadResult
                                                          {
                                                              ContentType = att.ContentType ?? "application/octet-stream",
                                                              Content = att.Content
                                                          })
                                           .FirstAsync(cancellationToken);
        return attachment;
    }
}