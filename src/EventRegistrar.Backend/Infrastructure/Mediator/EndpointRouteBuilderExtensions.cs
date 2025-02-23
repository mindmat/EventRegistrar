using System.Collections;
using System.Data;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using ClosedXML.Excel;
using ClosedXML.Graphics;

using EventRegistrar.Backend.Properties;

using SimpleInjector;


namespace EventRegistrar.Backend.Infrastructure.Mediator;

public static class EndpointRouteBuilderExtensions
{
    private static readonly JsonSerializerOptions _jsonSettings;
    private static readonly JsonSerializerOptions _jsonDeserializeSettings;
    private static readonly Type[] _exportableDataTypes = [typeof(string), typeof(int), typeof(int?), typeof(bool), typeof(bool?), typeof(decimal), typeof(decimal?)];

    static EndpointRouteBuilderExtensions()
    {
        // frontend expects enums as int, but sends enums as strings
        // -> use different settings
        _jsonSettings = new(JsonSerializerDefaults.Web);
        _jsonDeserializeSettings = new(JsonSerializerDefaults.Web);
        _jsonDeserializeSettings.Converters.Add(new JsonStringEnumConverter());
    }

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
        }
    }

    private static RequestDelegate CreateProcessRequest(Type requestType, Container container)
    {
        var openGenericMethod = typeof(EndpointRouteBuilderExtensions).GetMethod(nameof(CreateProcessRequestGeneric),
                                                                                 BindingFlags.Static | BindingFlags.NonPublic)!;
        var genericMethod = openGenericMethod.MakeGenericMethod(requestType);
        return (RequestDelegate)genericMethod.Invoke(null, [container])!;
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
                request = await JsonSerializer.DeserializeAsync<TRequest>(context.Request.Body, _jsonDeserializeSettings, context.RequestAborted);
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
        else if (context.Request.Headers.Accept == "text/plain" && response is string textResponse)
        {
            context.Response.Headers.ContentType = "text/plain";
            await context.Response.WriteAsync(textResponse, context.RequestAborted);
        }
        else
        {
            if (response is Unit)
            {
                context.Response.StatusCode = 204;
                await context.Response.Body.FlushAsync(context.RequestAborted);
            }
            else if(response is DownloadResult download)
            {
                context.Response.ContentType = download.ContentType;
                context.Response.StatusCode = 200;
                if (download.Filename != null)
                {
                    var filenameEncoded = WebUtility.UrlEncode(download.Filename);
                    context.Response.Headers.ContentDisposition = new ContentDisposition("attachment") { FileName = filenameEncoded }.ToString();
                }

                await context.Response.BodyWriter.WriteAsync(download.Content, context.RequestAborted);
                await context.Response.BodyWriter.FlushAsync(context.RequestAborted);
            }
            else
            {
                await SerializeAsJson(context, response, requestType);
                await context.Response.Body.FlushAsync(context.RequestAborted);
            }
        }
    }

    private static async Task SerializeAsJson(HttpContext context, object? response, Type requestType)
    {
        context.Response.Headers["content-type"] = "application/json";
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
        context.Response.Headers.ContentType = "application/octet-stream";
        LoadOptions.DefaultGraphicEngine = new DefaultGraphicEngine("DejaVu Sans");
        var workbook = new XLWorkbook();
        foreach (var (name, values, rowType) in GetEnumerableProperties(response))
        {
            var mappings = GetExportableProperties(rowType).ToList();

            var dataTable = new DataTable(name);

            foreach (var mapping in mappings)
            {
                dataTable.Columns.Add(new DataColumn(mapping.Name, mapping.GetValue.Method.ReturnType) { Caption = mapping.Caption });
            }

            if (values is null)
            {
                continue;
            }

            var isFirstRow = true;
            foreach (var dataRow in values)
            {
                if (isFirstRow)
                {
                    isFirstRow = false;
                    if (dataRow is IDynamicColumns firstRowWithDynamicColumns)
                    {
                        foreach (var dynamicColumn in firstRowWithDynamicColumns.DynamicColumns!)
                        {
                            mappings.Add(new ExportableProperty(dynamicColumn.Key, dynamicColumn.Key, (row => ((IDynamicColumns)row).DynamicColumns?[dynamicColumn.Key])));
                            dataTable.Columns.Add(dynamicColumn.Key);
                        }
                    }
                }

                var tableRow = dataTable.NewRow();
                foreach (var mapping in mappings)
                {
                    tableRow[mapping.Name] = FormatValue(mapping.GetValue(dataRow));
                }

                dataTable.Rows.Add(tableRow);
            }

            var worksheet = workbook.AddWorksheet(dataTable, name);
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

    private static IEnumerable<ExportableProperty> GetExportableProperties(Type type)
    {
        return type.GetProperties()
                   .Where(prp => _exportableDataTypes.Contains(prp.PropertyType))
                   .Select(prp => new ExportableProperty(prp.Name, GetTranslation(type.Name, prp.Name) ?? prp.Name, prp.GetValue));
    }

    private static string? GetTranslation(string typeName, string propertyName)
    {
        return Resources.ResourceManager.GetString($"{typeName}_{propertyName}");
    }

    private record ExportableProperty(string Name, string Caption, Func<object, object?> GetValue);

    private static IEnumerable<(string Name, IEnumerable? Values, Type)> GetEnumerableProperties(object? data)
    {
        if (data is IEnumerable enumerable)
        {
            return [("List", (IEnumerable?)enumerable, data.GetType().GetGenericArguments()[0])];
        }

        return data?.GetType()
                   .GetProperties()
                   .Where(prp => prp.PropertyType.IsGenericType
                              && prp.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>)))
                   .Select(prp => (prp.Name, prp.GetValue(data) as IEnumerable, prp.PropertyType.GetGenericArguments()[0]))
                   .ToList()
            ?? Enumerable.Empty<(string, IEnumerable?, Type)>();
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