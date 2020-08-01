
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure
{
    public class JsonHelper
    {
        private readonly JsonSerializerSettings _settings;

        public JsonHelper()
        {
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public T? TryDeserialize<T>(string json)
            where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
            catch
            {
                return null;
            }
        }

        public string Serialize<T>(T value)
            where T : class
        {
            return JsonConvert.SerializeObject(value, _settings);
        }
    }
}