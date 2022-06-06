namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class HttpContextContainer : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}