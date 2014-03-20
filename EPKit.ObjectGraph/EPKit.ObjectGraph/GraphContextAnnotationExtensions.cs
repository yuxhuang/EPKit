using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EPKit.ObjectGraph.Annotations;

namespace EPKit.ObjectGraph
{
    public static class GraphContextAnnotationExtensions
    {
        public static IEnumerable<PropertyInfo> GetGraphProperties(this Type type, Type attributeType)
        {
            var props = type.GetProperties(BindingFlags.Public);

            return props.Where(i => i.GetCustomAttribute(attributeType) != null).ToList();
        }

        public static bool IsGraphProperty(this PropertyInfo info, Type attributeType)
        {
            return info.GetCustomAttribute(attributeType) != null;
        }
        
        public static string GetIdentifier<T>(this T entity) where T : class, new()
        {
            var prop = typeof (T).GetGraphProperties(typeof (IdentifierAttribute)).FirstOrDefault();
            
            if (prop == null)
                return null;

            return prop.GetValue(entity).ToString();
        }
    }
}
