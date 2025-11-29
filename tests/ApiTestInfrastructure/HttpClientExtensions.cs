using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using MediatR;

namespace ApiTestInfrastructure;

public static class HttpClientExtensions
{
    public static async Task<ServiceResult<TResult>> Execute<TQuery, TResult>(this HttpClient client,
                                                                              TQuery query,
                                                                              bool throwExceptionOnScheduledJobs = false)
        where TQuery : IRequest<TResult>
        where TResult : class
    {
        var json = JsonSerializer.Serialize(query, _options);

        Task<HttpResponseMessage> Post()
        {
            return client.PostAsync($"api/{typeof(TQuery).Name}", new StringContent(json, Encoding.UTF8, "application/json"));
        }

        var response = await Post();

        if (!response.IsSuccessStatusCode)
        {
            return await CreateErrorServiceResult<TResult>(response).ConfigureAwait(false);
        }

        return new ServiceResult<TResult>(response.StatusCode, await response.Content.ReadFromJsonAsync<TResult>(_options));
    }

    private static readonly JsonSerializerOptions _options = new()
                                                             {
                                                                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                                 Converters =
                                                                 {
                                                                     new JsonStringEnumConverter()
                                                                 }
                                                             };
}