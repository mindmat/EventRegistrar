using System.Reflection;
using System.Text.Json;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

using MediatR;

using SimpleInjector;


namespace EventRegistrar.Backend.Infrastructure.Mediator;

public static class EndpointRouteBuilderExtensions
{
    private static readonly JsonSerializerOptions _jsonSettings = new(JsonSerializerDefaults.Web);

    public static void MapRequests(this IEndpointRouteBuilder endpointsBuilder, Container container)
    {
        var requests = container.GetInstance<RequestRegistry>();

        foreach (var request in requests.RequestTypes)
        {
            endpointsBuilder.MapPost($"/api/{request.Request.Name}", CreateProcessRequest(request.Request, container))
                            .RequireAuthorization()
                            //.RequireCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyHeader())
                            .WithDisplayName(request.Request.Name)
                            .WithMetadata(request);
            //for (var i = 0; i < endpoint.Metadata.Count; i++) builder.WithMetadata(endpoint.Metadata[i]);
        }
    }

    private static RequestDelegate CreateProcessRequest(Type requestType, Container container)
    {
        var openGenericMethod = typeof(EndpointRouteBuilderExtensions).GetMethod(nameof(CreateProcessRequestGeneric), BindingFlags.Static | BindingFlags.NonPublic)!;
        var genericMethod = openGenericMethod.MakeGenericMethod(requestType);
        return (RequestDelegate)genericMethod.Invoke(null, new object?[] { container })!;
    }

    private static RequestDelegate CreateProcessRequestGeneric<TRequest>(Container container)
        where TRequest : IBaseRequest
    {
        return context => ProcessRequest<TRequest>(context, container);
    }

    private static async Task ProcessRequest<TRequest>(HttpContext context, Container container)
        where TRequest : IBaseRequest
    {
        var requestType = typeof(TRequest);

        //var optionsAccessor = context.RequestServices.GetService<IOptions<MediatorEndpointOptions>>();
        //var options = optionsAccessor.Value;
        IBaseRequest? request;
        if (context.Request.ContentLength.GetValueOrDefault() != 0)
        {
            //try
            {
                request = await JsonSerializer.DeserializeAsync<TRequest>(context.Request.Body, _jsonSettings, context.RequestAborted);
                //MapRouteData(requestMetadata, context.GetRouteData(), model);
            }
            //catch (JsonException exception)
            //{
            //    context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //    await options.OnDeserializeError(context, exception);
            //    return;
            //}
            //catch (Exception exception) when (exception is FormatException || exception is OverflowException)
            //{
            //    context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //    await options.OnDeserializeError(context, exception);
            //    return;
            //}
        }
        else
        {
            request = Activator.CreateInstance(requestType) as IBaseRequest;
            //MapRouteData(requestMetadata, context.GetRouteData(), model);
        }

        using var scope = new EnsureExecutionScope(container);
        container.GetInstance<IHttpContextAccessor>().HttpContext = context;
        var authorizationChecker = container.GetInstance<IAuthorizationChecker>();
        if (request is IEventBoundRequest eventBoundRequest)
        {
            await authorizationChecker.ThrowIfUserHasNotRight(eventBoundRequest.EventId, requestType.Name);
        }

        var mediator = container.GetInstance<IMediator>();
        var response = await mediator.Send(request, context.RequestAborted);
        var dbContext = container.GetInstance<DbContext>();
        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }

        // "transaction": only release messages to event bus if db commit succeeds
        await container.GetInstance<ServiceBusClient>().Release();


        context.Response.Headers.Add("content-type", "application/json");

        var objectType = response?.GetType() ?? requestType;
        await JsonSerializer.SerializeAsync(context.Response.Body, response, objectType, _jsonSettings, context.RequestAborted);

        await context.Response.Body.FlushAsync(context.RequestAborted);
    }


    //private static async Task MediatorRequestDelegate(HttpContext context)
    //{
    //    var endpoint = context.GetEndpoint();

    //    var requestType = endpoint?.Metadata.GetMetadata<Type>();

    //    //var optionsAccessor = context.RequestServices.GetService<IOptions<MediatorEndpointOptions>>();
    //    //var options = optionsAccessor.Value;
    //    IBaseRequest? request;
    //    if (context.Request.ContentLength.GetValueOrDefault() != 0)
    //    {
    //        //try
    //        {
    //            request = await JsonSerializer.DeserializeAsync(context.Request.Body, requestType, _jsonSettings, context.RequestAborted) as IBaseRequest;
    //            //MapRouteData(requestMetadata, context.GetRouteData(), model);
    //        }
    //        //catch (JsonException exception)
    //        //{
    //        //    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    //        //    await options.OnDeserializeError(context, exception);
    //        //    return;
    //        //}
    //        //catch (Exception exception) when (exception is FormatException || exception is OverflowException)
    //        //{
    //        //    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    //        //    await options.OnDeserializeError(context, exception);
    //        //    return;
    //        //}
    //    }
    //    else
    //    {
    //        request = Activator.CreateInstance(requestType) as IBaseRequest;
    //        //MapRouteData(requestMetadata, context.GetRouteData(), model);
    //    }

    //    var container = context.RequestServices.GetService<Container>();
    //    var mediator = container.GetInstance<IMediator>();

    //    var response = await mediator.Send(request, context.RequestAborted);

    //    context.Response.Headers.Add("content-type", "application/json");

    //    var objectType = response?.GetType() ?? requestType;
    //    await JsonSerializer.SerializeAsync(context.Response.Body, response, objectType, _jsonSettings, context.RequestAborted);

    //    await context.Response.Body.FlushAsync(context.RequestAborted);
    //}

    //private static void MapRouteData(IMediatorEndpointMetadata requestMetadata, RouteData routeData, object model)
    //{
    //    if (model == null || routeData == null || routeData.Values.Count == 0)
    //    {
    //        return;
    //    }

    //    var properties = requestMetadata.RequestType.GetProperties();
    //    foreach (var item in routeData.Values)
    //        for (var i = 0; i < properties.Length; i++)
    //        {
    //            var property = properties[i];
    //            if (property.Name.Equals(item.Key, StringComparison.InvariantCultureIgnoreCase))
    //            {
    //                var value = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(item.Value.ToString());
    //                property.SetValue(model, value);
    //                break;
    //            }
    //        }
    //}
}