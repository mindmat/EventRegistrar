using System.Net;

namespace EventRegistrar.Backend.Infrastructure.ErrorHandling;

public interface IExceptionTranslation
{
    Type ExceptionType { get; }
    Predicate<Exception> Filter { get; }
    HttpStatusCode HttpCode { get; }
    Func<Exception, object> SelectMessage { get; }
}