using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Mailing.Templates;

public class AvailableMailersQuery : IEventBoundRequest, IRequest<IEnumerable<MailSender>>
{
    public Guid EventId { get; set; }
}

public class AvailableMailersQueryHandler(SecretReader secretReader) : IRequestHandler<AvailableMailersQuery, IEnumerable<MailSender>>
{
    public async Task<IEnumerable<MailSender>> Handle(AvailableMailersQuery query, CancellationToken cancellationToken)
    {
        var availableMailers = new List<MailSender>();
        if (await secretReader.GetSendGridApiKey(cancellationToken) != null)
        {
            availableMailers.Add(MailSender.SendGrid);
        }

        if (await secretReader.GetPostmarkToken(cancellationToken) != null)
        {
            availableMailers.Add(MailSender.Postmark);
        }

        return availableMailers;
    }
}