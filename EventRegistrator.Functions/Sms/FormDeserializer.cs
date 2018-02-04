using System.Collections.Specialized;
using System.Linq;

namespace EventRegistrator.Functions.Sms
{
    public class FormDeserializer
    {
        public static T Read<T>(NameValueCollection formData) 
            where T : new()
        {
            var properties = typeof(T).GetProperties();
            var deserialized = new T();
            foreach (var property in properties.Where(prp=>formData.AllKeys.Contains(prp.Name)))
            {
                property.SetValue(deserialized, formData[property.Name]);
            }
            return deserialized;
        }
    }
}