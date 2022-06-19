using System.Net;

using EventRegistrar.Backend.Infrastructure.ErrorHandling;

namespace EventRegistrar.Backend.Authentication;

public class UnauthenticatedExceptionTranslation : ExceptionTranslation<UnauthorizedAccessException>
{
    public UnauthenticatedExceptionTranslation()
    {
        HttpCode = HttpStatusCode.Unauthorized;
    }

    public override object Translate(UnauthorizedAccessException ex)
    {
        return ex.Message;
    }
}