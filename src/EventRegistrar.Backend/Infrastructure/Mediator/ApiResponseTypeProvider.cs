using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace EventRegistrar.Backend.Infrastructure.Mediator;

internal class ApiResponseTypeProvider
{
    public ICollection<ApiResponseType> GetApiResponseTypes(ApiDescription action, Type requestType)
    {
        var declaredReturnType = requestType.GetInterface(typeof(IRequest<>).Name)?.GetGenericArguments()[0]!;
        var serializedJsonType = declaredReturnType.IsGenericType
                                     ? declaredReturnType?.GetGenericTypeDefinition()
                                     : declaredReturnType;
        if (serializedJsonType == typeof(SerializedJson<>))
        {
            declaredReturnType = declaredReturnType!.GetGenericArguments()[0];
        }

        return GetApiResponseTypes(declaredReturnType);
    }

    //private static List<IApiResponseMetadataProvider> GetResponseMetadataAttributes(ApiDescription action)
    //{
    //    if (action.FilterDescriptors == null)
    //    {
    //        return new List<IApiResponseMetadataProvider>();
    //    }

    //    // This technique for enumerating filters will intentionally ignore any filter that is an IFilterFactory
    //    // while searching for a filter that implements IApiResponseMetadataProvider.
    //    //
    //    // The workaround for that is to implement the metadata interface on the IFilterFactory.
    //    return action.FilterDescriptors
    //                 .Select(fd => fd.Filter)
    //                 .OfType<IApiResponseMetadataProvider>()
    //                 .ToList();
    //}

    private static ICollection<ApiResponseType> GetApiResponseTypes(Type? type)
    {
        var results = new Dictionary<int, ApiResponseType>();

        // Get the content type that the action explicitly set to support.
        // Walk through all 'filter' attributes in order, and allow each one to see or override
        // the results of the previous ones. This is similar to the execution path for content-negotiation.
        var contentTypes = new MediaTypeCollection();

        // Set the default status only when no status has already been set explicitly
        if (results.Count == 0 && type != null)
        {
            results[StatusCodes.Status200OK] = new ApiResponseType
                                               {
                                                   StatusCode = StatusCodes.Status200OK,
                                                   Type = type
                                               };
        }

        if (contentTypes.Count == 0)
        {
            // None of the IApiResponseMetadataProvider specified a content type. This is common for actions that
            // specify one or more ProducesResponseType but no ProducesAttribute. In this case, formatters will participate in conneg
            // and respond to the incoming request.
            // Querying IApiResponseTypeMetadataProvider.GetSupportedContentTypes with "null" should retrieve all supported
            // content types that each formatter may respond in.
            contentTypes.Add((string?)null);
        }

        var responseTypes = results.Values;
        CalculateResponseFormats(responseTypes, contentTypes);
        return responseTypes;
    }

    private static void CalculateResponseFormats(IEnumerable<ApiResponseType> responseTypes, MediaTypeCollection declaredContentTypes)
    {
        foreach (var apiResponse in responseTypes)
        {
            var responseType = apiResponse.Type;
            if (responseType == null || responseType == typeof(void))
            {
                continue;
            }

            foreach (var contentType in declaredContentTypes)
            {
                apiResponse.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "application/json" });

                if (contentType != null)
                {
                    // No output formatter was found that supports this content type. Add the user specified content type as-is to the result.
                    apiResponse.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = contentType });
                }
            }
        }
    }
}