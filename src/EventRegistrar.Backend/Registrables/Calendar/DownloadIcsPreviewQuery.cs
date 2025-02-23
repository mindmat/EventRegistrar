using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Mailing.Compose;

namespace EventRegistrar.Backend.Registrables.Calendar;

public class DownloadIcsPreviewQuery : IRequest<DownloadResult>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class DownloadMailAttachmentQueryHandler(IcsCreator _icsCreator) : IRequestHandler<DownloadIcsPreviewQuery, DownloadResult>
{
    public async Task<DownloadResult> Handle(DownloadIcsPreviewQuery query, CancellationToken cancellationToken)
    {
        var attachment = await _icsCreator.Create(query.RegistrationId, cancellationToken);
        return attachment != null
                   ? new DownloadResult
                     {
                         Filename = attachment.Name,
                         ContentType = attachment.ContentType ?? "application/octet-stream",
                         Content = attachment.Content
                     }
                   : throw new ApplicationException("No spots with a schedule");
    }
}
