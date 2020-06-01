
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure
{
    public class JsonHelper
    {
        public static T? TryDeserialize<T>(string json)
            where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}