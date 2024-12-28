using EventRegistrar.Backend.Infrastructure.ErrorHandling;
using System.Diagnostics;

using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Infrastructure.AuditLog
{
    public class AuditLogger<TRequest, TResponse>(RequestDateTimeProvider dateTimeProvider,
                                                  AuthenticatedUser user,
                                                  AuthenticatedUserId userId,
                                                  JsonHelper jsonHelper,
                                                  ExceptionTranslator exceptionTranslator,
                                                  AuditLogDbContext dbContext,
                                                  EventContext eventContext) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class, IBaseRequest
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var isCommand = request is IRequest;
            if (!isCommand)
            {
                return await next();
            }

            var requestAuditId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();
            string? exception = null;
            var startTime = dateTimeProvider.RequestNow;
            try
            {
                var result = await next().ConfigureAwait(false);
                stopwatch.Stop();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                exception = exceptionTranslator.TranslateExceptionToUserText(ex).result as string ?? ex.Message;
                throw;
            }
            finally
            {
                try
                {
                    var requestAudit = new RequestLog
                    {
                        Id = requestAuditId,
                        RequestType = request.GetType().Name,
                        RequestJson = jsonHelper.TrySerialize(request) ?? "Error",
                        EventId = eventContext.EventId,
                        UserId = userId.UserId,
                        When = startTime,
                        UserDisplayText = user.GetText(),
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
    }
}