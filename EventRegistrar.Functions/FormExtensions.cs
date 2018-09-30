using System.Linq;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Functions
{
    public static class FormExtensions
    {
        public static T Deserialize<T>(this IFormCollection form)
            where T : new()
        {
            var properties = typeof(T).GetProperties();
            var deserialized = new T();
            foreach (var property in properties.Where(prp => form.ContainsKey(prp.Name)))
            {
                property.SetValue(deserialized, form[property.Name]);
            }
            return deserialized;
        }
    }
}