using System.Text.RegularExpressions;

using EventRegistrar.Backend.Authentication.Users;
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

public class RequestTypeDisplayItem
{
    public string RequestType { get; set; } = null!;
    public string RequestTypeText { get; set; } = null!;
}

public class RequestLogQuery : IRequest<IEnumerable<RequestLogDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string[]? IncludeRequestTypes { get; set; }
    public string[]? ExcludeRequestTypes { get; set; }
    public string? SearchString { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public bool? OnlyWithErrors { get; set; }
    public bool? OnlyUserInitiatedRequests { get; set; }
}

public class RequestLogQueryHandler(AuditLogDbContext dbContext,
                                    IQueryable<User> users)
    : IRequestHandler<RequestLogQuery, IEnumerable<RequestLogDisplayItem>>
{
    public async Task<IEnumerable<RequestLogDisplayItem>> Handle(RequestLogQuery query,
                                                                 CancellationToken cancellationToken)
    {
        var excludeRequestTypes = query.ExcludeRequestTypes ?? [];
        var includeRequestTypes = query.IncludeRequestTypes ?? [];
        var logs = dbContext.Set<RequestLog>()
                            .Where(log => log.EventId == query.EventId)
                            .WhereIf(excludeRequestTypes.Any(),
                                     log => !excludeRequestTypes.Contains(log.RequestType))
                            .WhereIf(includeRequestTypes.Any(),
                                     log => includeRequestTypes.Contains(log.RequestType))
                            .WhereIf(query.From.HasValue,
                                     log => log.When >= query.From!.Value)
                            .WhereIf(query.To.HasValue,
                                     log => log.When <= query.To!.Value)
                            .WhereIf(query.OnlyWithErrors == true,
                                     log => log.Exception != null)
                            .WhereIf(query.OnlyUserInitiatedRequests == true,
                                     log => log.UserId != null)
                            .WhereIf(!string.IsNullOrEmpty(query.SearchString),
                                     log => log.RequestJson.Contains(query.SearchString!)
                                         || log.UserDisplayText!.Contains(query.SearchString!)
                                         || log.RequestType.Contains(query.SearchString!));

        var rawLogs = await logs.OrderByDescending(e => EF.Property<long>(e, RequestLogMap.SequencePropertyName))
                                .Take(100)
                                .Select(log => new
                                               {
                                                   log.Id,
                                                   log.RequestType,
                                                   log.RequestJson,
                                                   log.When,
                                                   log.UserId,
                                                   log.Exception,
                                                   log.ExecutionTimeInMilliseconds
                                               })
                                .ToListAsync(cancellationToken);
        var userIds = rawLogs.Select(log => log.UserId)
                             .WhereNotNull()
                             .Distinct();
        var userLookup = await users.Where(usr => userIds.Contains(usr.Id))
                                    .ToDictionaryAsync(usr => usr.Id,
                                                       usr => $"{usr.FirstName} {usr.LastName}",
                                                       cancellationToken);
        return rawLogs.Select(log => new RequestLogDisplayItem
                                     {
                                         Id = log.Id,
                                         RequestType = log.RequestType,
                                         RequestTypeText = RequestLogTextTranslator.TranslateRequestType(log.RequestType),
                                         RequestJson = log.RequestJson,
                                         When = log.When,
                                         User = userLookup.LookupNullable(log.UserId),
                                         Exception = log.Exception,
                                         ExecutionTimeInMilliseconds = log.ExecutionTimeInMilliseconds
                                     });
    }
}

public class RequestLogRequestTypesQuery : IRequest<IEnumerable<RequestTypeDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RequestLogRequestTypesQueryHandler(AuditLogDbContext dbContext)
    : IRequestHandler<RequestLogRequestTypesQuery, IEnumerable<RequestTypeDisplayItem>>
{
    public async Task<IEnumerable<RequestTypeDisplayItem>> Handle(RequestLogRequestTypesQuery query,
                                                                  CancellationToken cancellationToken)
    {
        var requestTypes = await dbContext.Set<RequestLog>()
                                          .Where(log => log.EventId == query.EventId)
                                          .Select(log => log.RequestType)
                                          .Distinct()
                                          .OrderBy(requestType => requestType)
                                          .ToListAsync(cancellationToken);

        return requestTypes.Select(requestType => new RequestTypeDisplayItem
                                                  {
                                                      RequestType = requestType,
                                                      RequestTypeText = RequestLogTextTranslator.TranslateRequestType(requestType)
                                                  });
    }
}

internal static class RequestLogTextTranslator
{
    public static string TranslateRequestType(string requestType)
    {
        var template = Resources.ResourceManager.GetString(requestType);
        if (template == null)
        {
            return requestType;
        }

        return Regex.Replace(template, @"\{\{(\w+)\}\}", string.Empty)
                    .Trim();
    }
}