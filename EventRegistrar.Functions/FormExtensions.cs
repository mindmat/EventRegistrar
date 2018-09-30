using System.Linq;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Functions
{
    public static class FormExtensions
    {
        public static T Deserialize<T>(this IFormCollection form)
            where T : new()
        {
            var targetProperties = typeof(T).GetProperties();
            var deserialized = new T();
            foreach (var property in targetProperties.Where(prp => form.ContainsKey(prp.Name)))
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(deserialized, form[property.Name].ToString());
                }
            }
            return deserialized;
        }
    }
}