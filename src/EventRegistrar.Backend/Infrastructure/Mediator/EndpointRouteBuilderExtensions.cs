using System.Collections;
using System.Data;
using System.Reflection;
using System.Text.Json;

using ClosedXML.Excel;
using ClosedXML.Graphics;

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

        if (context.Request.Headers.Accept == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            await SerializeAsXlsx(context, response, container.GetInstance<ILogger>());
        }
        else
        {
            await SerializeAsJson(context, response, requestType);
        }

        await context.Response.Body.FlushAsync(context.RequestAborted);
    }

    private static async Task SerializeAsJson(HttpContext context, object? response, Type requestType)
    {
        context.Response.Headers.Add("content-type", "application/json");
        if (response is ISerializedJson serializedJson)
        {
            await context.Response.WriteAsync(serializedJson.Content, context.RequestAborted);
        }
        else
        {
            var objectType = response?.GetType() ?? requestType;
            await JsonSerializer.SerializeAsync(context.Response.Body, response, objectType, _jsonSettings, context.RequestAborted);
        }
    }

    private static async Task SerializeAsXlsx(HttpContext context, object? response, ILogger logger)
    {
        // try to serialize as xlsx
        context.Response.Headers.Add("content-type", "application/octet-stream");
        LoadOptions.DefaultGraphicEngine = new DefaultGraphicEngine("DejaVu Sans");
        var workbook = new XLWorkbook();
        foreach (var (propertyInfo, rowType) in GetEnumerableProperties(response))
        {
            var mappings = GetExportableProperties(rowType)
                           .Select(prp => (prp.Name, (Func<object, object?>)prp.GetValue))
                           .ToList();

            var dataTable = new DataTable(propertyInfo.Name);

            foreach (var (title, _) in mappings)
            {
                dataTable.Columns.Add(title);
            }

            if (propertyInfo.GetValue(response) is not IEnumerable dataRows)
            {
                continue;
            }

            foreach (var dataRow in dataRows)
            {
                var tableRow = dataTable.NewRow();
                foreach (var (title, getValue) in mappings)
                {
                    tableRow[title] = FormatValue(getValue(dataRow));
                }

                dataTable.Rows.Add(tableRow);
            }

            var worksheet = workbook.AddWorksheet(dataTable, propertyInfo.Name);
            try
            {
                worksheet.Columns().AdjustToContents();
            }
            catch
            {
                // sandbox
                foreach (var fontFamily in SixLabors.Fonts.SystemFonts.Collection.Families)
                {
                    logger.LogInformation("Font available: {name}", fontFamily.Name);
                }
            }
            //worksheet.Sort(1);
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        await context.Response.BodyWriter.WriteAsync(stream.ToArray());
    }

    private static object? FormatValue(object? value)
    {
        if (value is bool valueBool)
        {
            return valueBool
                       ? "x"
                       : string.Empty;
        }

        return value;
    }

    private static IEnumerable<PropertyInfo> GetExportableProperties(Type type)
    {
        return type.GetProperties()
                   .Where(prp => prp.PropertyType == typeof(string)
                              || prp.PropertyType == typeof(int)
                              || prp.PropertyType == typeof(bool));
    }

    private static IEnumerable<(PropertyInfo, Type)> GetEnumerableProperties(object? data)
    {
        return data?.GetType()
                   .GetProperties()
                   .Where(prp => prp.PropertyType.IsGenericType
                              && prp.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>)))
                   .Select(prp => (prp, prp.PropertyType.GetGenericArguments()[0]))
                   .ToList()
            ?? Enumerable.Empty<(PropertyInfo, Type)>();
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