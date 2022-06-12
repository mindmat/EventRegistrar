using System.Net;

namespace EventRegistrar.Backend.Infrastructure.ErrorHandling;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ExceptionTranslator _exceptionTranslator;
    private const string TranslatedExceptionKey = "TranslatedException";

    public ExceptionMiddleware() { }
    //public ExceptionMiddleware(ExceptionTranslator exceptionTranslator)
    //{
    //    _exceptionTranslator = exceptionTranslator;
    //}

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleException(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        if (exception is AggregateException aggregateException)
        {
            var errors = aggregateException
                         .InnerExceptions
                         .Select(ex => _exceptionTranslator.TranslateExceptionToUserText(ex).result as string ?? ex.Message)
                         .StringJoin(Environment.NewLine);
            context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
            await context.Response.WriteAsync(errors).ConfigureAwait(false);
            //_telemetryClient.TrackException(exception, new Dictionary<string, string> { { TranslatedExceptionKey, errors } });
        }
        else
        {
            var (result, httpCode) = _exceptionTranslator.TranslateExceptionToUserText(exception);
            context.Response.StatusCode = (int)(httpCode ?? HttpStatusCode.InternalServerError);
            switch (result)
            {
                case string msg:
                    context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
                    await context.Response.WriteAsync(msg).ConfigureAwait(false);
                    //_telemetryClient.TrackException(exception, new Dictionary<string, string> { { TranslatedExceptionKey, msg } });
                    break;
            }
        }
    }
}