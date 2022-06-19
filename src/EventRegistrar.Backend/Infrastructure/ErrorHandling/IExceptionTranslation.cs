using System.Net;

namespace EventRegistrar.Backend.Infrastructure.ErrorHandling;

public interface IExceptionTranslation
{
    Type ExceptionType { get; }
    Predicate<Exception> Filter { get; }
    HttpStatusCode HttpCode { get; }
    Func<Exception, object> SelectMessage { get; }
}

public abstract class ExceptionTranslation<TException> : IExceptionTranslation
    where TException : Exception
{
    Type IExceptionTranslation.ExceptionType => typeof(TException);

    Predicate<Exception> IExceptionTranslation.Filter => ex => ex is TException e && Filter(e);

    public HttpStatusCode HttpCode { get; protected set; } = HttpStatusCode.BadRequest;

    Func<Exception, object> IExceptionTranslation.SelectMessage => ex => ex is not TException e
                                                                             ? ex.Message
                                                                             : Translate(e);

    public virtual bool Filter(TException ex)
    {
        // default: take all Exceptions of Type TException
        return true;
    }

    public abstract object Translate(TException ex);
}