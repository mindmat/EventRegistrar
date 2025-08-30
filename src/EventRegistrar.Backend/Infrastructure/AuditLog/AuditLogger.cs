using EventRegistrar.Backend.Infrastructure.ErrorHandling;
using System.Diagnostics;

using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace EventRegistrar.Backend.Infrastructure.AuditLog;

public class AuditLogger<TRequest, TResponse>(RequestDateTimeProvider dateTimeProvider,
                                              AuthenticatedUser user,
                                              AuthenticatedUserId userId,
                                              JsonHelper jsonHelper,
                                              ExceptionTranslator exceptionTranslator,
                                              AuditLogDbContext dbContext,
                                              EventContext eventContext,
                                              TelemetryClient telemetryClient) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IBaseRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IRequest command)
        {
            return await next();
        }

        var requestType = request.GetType().Name;
        var requestAuditId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();
        string? exception = null;
        var startTime = dateTimeProvider.RequestNow;
        using var operation = telemetryClient.StartOperation<RequestTelemetry>(requestType);
        try
        {
            var result = await next().ConfigureAwait(false);
            stopwatch.Stop();
            operation.Telemetry.Success=true;
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            exception = exceptionTranslator.TranslateExceptionToUserText(ex).result as string ?? ex.Message;
            operation.Telemetry.Success = false;
            operation.Telemetry.Properties.Add("Error", exception);
            throw;
        }
        finally
        {
            try
            {
                operation.Telemetry.Stop();
                var requestJson = jsonHelper.TrySerialize(request);
                var userDisplayText = user.GetText();
                var eventId = eventContext.EventId ?? TryGetEventId(command)!;

                if (requestJson != null)
                {
                    operation.Telemetry.Properties.Add("Request", requestJson);
                }
                operation.Telemetry.Properties.Add("EventId", eventId?.ToString());
                operation.Telemetry.Properties.Add("UserId", userId.UserId?.ToString());
                operation.Telemetry.Properties.Add("Username", userDisplayText);


                var requestAudit = new RequestLog
                                   {
                                       Id = requestAuditId,
                                       RequestType = requestType,
                                       RequestJson = requestJson ?? "Error",
                                       EventId = eventId,
                                       UserId = userId.UserId,
                                       When = startTime,
                                       UserDisplayText = userDisplayText,
                                       Exception = exception,
                                       ExecutionTimeInMilliseconds = stopwatch.ElapsedMilliseconds
                                   };
                await dbContext.Set<RequestLog>().AddAsync(requestAudit, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }
    }

    private static Guid? TryGetEventId(dynamic command)
    {
        try
        {
            return command.EventId as Guid?;
        }
        catch
        {
            return null;
        }
    }
}