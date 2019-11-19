using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Test.Infrastructure
{
    public static class HttpClientExtensions
    {

        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            return await client.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"));
        }
    }
}