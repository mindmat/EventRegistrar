using System.Reflection;

using MediatR;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class MediatorEndpointApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
{
    private readonly RequestRegistry _requestRegistry;

    public MediatorEndpointApiDescriptionGroupCollectionProvider(RequestRegistry requestRegistry)
    {
        _requestRegistry = requestRegistry;
    }

    public int Order => 1;

    public ApiDescriptionGroupCollection ApiDescriptionGroups
    {
        get
        {
            var apis = new List<ApiDescription>();
            var openGenericRequestHandlerType = typeof(IRequestHandler<,>);
            foreach (var requestType in _requestRegistry.RequestTypes)
            {
                var controllerActionDescriptor = new ControllerActionDescriptor
                                                 {
                                                     DisplayName = requestType.Request.Name + "-D",
                                                     ActionName = requestType.Request.Name + "-A",
                                                     ControllerName = requestType.Request.Name + "-C",
                                                     ControllerTypeInfo = requestType.RequestHandler.GetTypeInfo(),
                                                     MethodInfo = requestType.RequestHandler.GetMethod("Handle")!,
                                                     Parameters = new List<ParameterDescriptor>
                                                                  {
                                                                      new()
                                                                      {
                                                                          Name = requestType.Request.Name,
                                                                          ParameterType = requestType.Request
                                                                      }
                                                                  }
                                                 };
                //var desc = new ActionDescriptor
                //           {
                //               DisplayName = requestType.Name + "-D",
                //               Parameters = requestType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                //                                       .Select(prp => new ParameterDescriptor
                //                                       {
                //                                           Name = prp.Name,
                //                                           ParameterType = prp.PropertyType
                //                                       })
                //                                       .ToList()
                //};

                var apiDescription = new ApiDescription
                                     {
                                         GroupName = "Mediator",
                                         HttpMethod = "Post",
                                         RelativePath = "/" + requestType.Request.Name,
                                         ActionDescriptor = controllerActionDescriptor,
                                         ParameterDescriptions =
                                         {
                                             new ApiParameterDescription
                                             {
                                                 Name = requestType.Request.Name,
                                                 Type = requestType.Request,
                                                 Source = BindingSource.Body
                                             }
                                         },
                                         SupportedRequestFormats = { new ApiRequestFormat { MediaType = "application/json" } }
                                         //SupportedResponseTypes = { new ApiResponseType { Type = typeof(string), IsDefaultResponse = true } }
                                     };

                var responseTypes = new ApiResponseTypeProvider().GetApiResponseTypes(apiDescription, requestType.Request);

                foreach (var responseType in responseTypes)
                {
                    apiDescription.SupportedResponseTypes.Add(responseType);
                }

                apis.Add(apiDescription);
            }

            var group = new ApiDescriptionGroup("Mediator", apis);

            return new ApiDescriptionGroupCollection(new[] { group }, 1);
        }
    }
}