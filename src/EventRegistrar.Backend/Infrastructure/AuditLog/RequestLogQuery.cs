using System.Text.Json;
using System.Text.RegularExpressions;

using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Infrastructure.AuditLog;

public class RequestLogDisplayItem
{
    public Guid Id { get; set; }
    public string RequestType { get; set; } = null!;
    public string RequestTypeText { get; set; } = null!;
    public string RequestJson { get; set; } = null!;
    public DateTimeOffset When { get; set; }
    public string? User { get; set; }
    public string? Exception { get; set; }
    public long ExecutionTimeInMilliseconds { get; set; }
}

public class RequestLogQuery : IRequest<IEnumerable<RequestLogDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? RequestType { get; set; }
    public string? SearchString { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public bool? OnlyWithErrors { get; set; }
}

public class RequestLogQueryHandler(AuditLogDbContext dbContext)
    : IRequestHandler<RequestLogQuery, IEnumerable<RequestLogDisplayItem>>
{
    public async Task<IEnumerable<RequestLogDisplayItem>> Handle(RequestLogQuery query,
                                                                 CancellationToken cancellationToken)
    {
        var logs = dbContext.Set<RequestLog>()
                            .Where(log => log.EventId == query.EventId)
                            .WhereIf(!string.IsNullOrEmpty(query.RequestType),
                                     log => log.RequestType == query.RequestType)
                            .WhereIf(query.From.HasValue,
                                     log => log.When >= query.From!.Value)
                            .WhereIf(query.To.HasValue,
                                     log => log.When <= query.To!.Value)
                            .WhereIf(query.OnlyWithErrors == true,
                                     log => log.Exception != null)
                            .WhereIf(!string.IsNullOrEmpty(query.SearchString),
                                     log => log.RequestJson.Contains(query.SearchString!)
                                         || log.UserDisplayText!.Contains(query.SearchString!)
                                         || log.RequestType.Contains(query.SearchString!));

        var rawLogs = await logs.OrderByDescending(log => log.When)
                                .Take(100)
                                .Select(log => new
                                               {
                                                   log.Id,
                                                   log.RequestType,
                                                   log.RequestJson,
                                                   log.When,
                                                   log.UserDisplayText,
                                                   log.Exception,
                                                   log.ExecutionTimeInMilliseconds
                                               })
                                .ToListAsync(cancellationToken);

        return rawLogs.Select(log => new RequestLogDisplayItem
                                     {
                                         Id = log.Id,
                                         RequestType = log.RequestType,
                                         RequestTypeText = TranslateRequestType(log.RequestType, log.RequestJson),
                                         RequestJson = log.RequestJson,
                                         When = log.When,
                                         User = log.UserDisplayText,
                                         Exception = log.Exception,
                                         ExecutionTimeInMilliseconds = log.ExecutionTimeInMilliseconds
                                     });
    }

    private static string TranslateRequestType(string requestType, string requestJson)
    {
        var template = Resources.ResourceManager.GetString(requestType);
        if (template == null)
        {
            return requestType;
        }

        try
        {
            using var document = JsonDocument.Parse(requestJson);
            return Regex.Replace(template, @"\{\{(\w+)\}\}", match =>
            {
                var propertyName = match.Groups[1].Value;
                return document.RootElement.TryGetProperty(propertyName, out var value)
                           ? value.ToString()
                           : match.Value;
            });
        }
        catch
        {
            return template;
        }
    }
}
