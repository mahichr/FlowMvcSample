using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

namespace NgFlowSample.Services
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a IDictionary<string, string> to a generic object of type <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static T ToObject<T>(this IDictionary<string, string> dict) where T : new()
        {
            T obj = new T();
            PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo property in properties)
            {
                if (!dict.Any(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                KeyValuePair<string, string> item = dict.First(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
                var tPropertyType = obj.GetType().GetProperty(property.Name);
                if (tPropertyType != null)
                {
                    Type newT = Nullable.GetUnderlyingType(tPropertyType.PropertyType) ?? tPropertyType.PropertyType;
                    object safeValue = null;
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        if (newT.IsEnum)
                        {
                            safeValue = Enum.Parse(newT, item.Value, true);
                        }
                        else
                        {
                            safeValue = Convert.ChangeType(item.Value, newT);
                        }
                    }
                    tPropertyType.SetValue(obj, safeValue, null);
                }
            }
            return obj;
        }

        /// <summary>
        /// Converts a NameValue Collection to a Dictionary
        /// </summary>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        /// <summary>
        /// Converts a NameValue collection to an object of type <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public static T ToObject<T>(this NameValueCollection nvc) where T : new()
        {
            return nvc.ToDictionary().ToObject<T>();
        }
    }
}
