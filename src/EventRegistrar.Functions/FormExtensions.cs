using System.Collections.Specialized;
using System.Linq;

namespace EventRegistrar.Functions;

public static class FormExtensions
{
    public static T Deserialize<T>(this NameValueCollection form)
        where T : new()
    {
        var targetProperties = typeof(T).GetProperties();
        var deserialized = new T();
        foreach (var property in targetProperties.Where(prp => form.AllKeys?.Contains(prp.Name) == true))
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(deserialized, form[property.Name]);
            }
        }

        return deserialized;
    }
}