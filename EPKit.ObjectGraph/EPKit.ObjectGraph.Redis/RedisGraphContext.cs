using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EPKit.ObjectGraph.Annotations;
using ServiceStack.Common;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace EPKit.ObjectGraph.Redis
{
    public class RedisGraphContext : IGraphContext, IDisposable
    {
        private readonly IRedisClient _client;

        private string GetKey(Type type, string id, string field = null)
        {
            return string.Format("{0}:{1}{2}", type.FullName, id,
                string.IsNullOrWhiteSpace(field) ? string.Empty : (":" + field));
        }

        private T Duplicate<T>(T value)
        {
            return JsonSerializer.DeserializeFromString<T>(JsonSerializer.SerializeToString(value));
        }

        public RedisGraphContext(IRedisClient client)
        {
            _client = client;
        }

        public async Task Persist<T>(T value) where T : class, new()
        {
            var obj = Duplicate(value);
            await Task.Run(() =>
            {
                using (var tx = _client.CreateTransaction())
                {
                    var id = obj.GetIdentifier();

                    // get all graph properties
                    var props = typeof(T).GetGraphProperties(typeof(GraphPropertyAttribute));

                    foreach (var prop in props)
                    {
                        var property = prop;
                        var propertyValue = property.GetValue(obj);
                        var key = GetKey(typeof(T), id, property.Name);

                        if (propertyValue != null)
                        {
                            if (property.IsGraphProperty(typeof(AssociatedAttribute)))
                            {
                                var propertyValueId = propertyValue.GetIdentifier();
                                tx.QueueCommand(c => c.Set(key, propertyValueId));
                                property.SetValue(obj, null);
                            }

                            if (property.IsGraphProperty(typeof(SetAttribute)))
                            {
                                
                                var set = (ISet<object>)propertyValue;
                                var setIds = set.Select(i => i.GetIdentifier()).ToList();
                                tx.QueueCommand(c => c.Sets[key].Clear());
                                foreach (var valueId in setIds)
                                {
                                    var id1 = valueId;
                                    tx.QueueCommand(c => c.Sets[key].Add(id1));
                                }
                                property.SetValue(obj, null);
                            }

                            if (property.IsGraphProperty(typeof(SortedSetAttribute)) || property.IsGraphProperty(typeof(CollectionAttribute)))
                            {
                                var list = (ICollection<object>)propertyValue;
                                var listIds = list.Select(i => i.GetIdentifier()).ToList();
                                tx.QueueCommand(c => c.Lists[key].Clear());
                                foreach (var valueId in listIds)
                                {
                                    var id1 = valueId;
                                    tx.QueueCommand(c => c.Lists[key].Add(id1));
                                }
                                property.SetValue(obj, null);
                            }

                            // TODO finish this

//                            if (prop.IsGraphProperty(typeof(HashAttribute)))
//                            {
//                                var key = GetKey(typeof(T), id, property.Name);
//                                tx.QueueCommand(c => c.Hashes[key].Clear());
//                                property.SetValue(obj, null);
//                            }
                        }
                    }

                    tx.QueueCommand(c => c.Set(GetKey(typeof(T), id), obj));
                    tx.Commit();
                }
            });
        }

        public Task Remove<T>(T value) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Task RemoveById<T>(string id) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetById<T>(string id, params Expression<Func<T, object>>[] includes) where T : class, new()
        {
            return await Task.Run(() =>
            {
                var result = _client.Get<T>(GetKey(typeof (T), id));

                if (result == null)
                    return null;

                var props = typeof(T).GetGraphProperties(typeof(GraphPropertyAttribute));

                var includedProperties = new List<PropertyInfo>();

                foreach (var include in includes)
                {
                    var propExpr = include.Body as MemberExpression;

                    if (propExpr == null)
                        throw new ArgumentException(string.Format("{0} is not a member expression.", include.Body));

                    var prop = propExpr.Member as PropertyInfo;

                    if (prop == null)
                        throw new ArgumentException(string.Format("{0}.{1} is not a property.", typeof(T), propExpr.Member.Name));

                    includedProperties.Add(prop);
                }

                foreach (var prop in props)
                {
                    var property = prop;
                    var key = GetKey(typeof (T), id, prop.Name);

                    // if the property is not included, we don't try to retrieve it.
                    if (!includedProperties.Contains(property))
                        continue;

                    if (property.IsGraphProperty(typeof (AssociatedAttribute)))
                    {
                        var valueId = _client.Get<string>(key);
                        var valueKey = GetKey(property.PropertyType, valueId);
                        var value = _client.GetValue(valueKey);
                        property.SetValue(result, JsonSerializer.DeserializeFromString(value, property.PropertyType));
                    }


                    if (property.IsGraphProperty(typeof(SetAttribute)) || property.IsGraphProperty(typeof(CollectionAttribute)) ||
                        property.IsGraphProperty(typeof(SortedSetAttribute)))
                    {
                        var collectionType = property.PropertyType;

                        if (!collectionType.IsGenericType)
                            throw new InvalidOperationException(
                                string.Format("Type {0}'s property {1} is not acceptable genereic set.", typeof(T), property.Name));

                        var parameterTypes = collectionType.GetGenericArguments();
                        // only check the first type
                        var itemType = parameterTypes[0];

                        IList<string> valueIds;
                        if (property.IsGraphProperty(typeof (SetAttribute)))
                            valueIds = _client.Sets[key].ToList();
                        else
                            valueIds = _client.Lists[key].ToList();

                        var valueKeys = valueIds.Select(i => GetKey(itemType, i)).ToList();

                        // get all items and fill them in
                        var valueStrings = _client.GetValues(valueKeys).Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
                        var values = valueStrings.Select(i => JsonSerializer.DeserializeFromString(i, itemType)).Where(i => i != null).ToList();

                        if (!values.Any())
                            continue;

                        // create a set
                        object instance = null;
                        if (collectionType.IsInterface)
                        {
                            if (property.IsGraphProperty(typeof(SetAttribute)))
                                instance = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(itemType));
                            if (property.IsGraphProperty(typeof (CollectionAttribute)))
                                instance = Activator.CreateInstance(typeof (List<>).MakeGenericType(itemType));
                            if (property.IsGraphProperty(typeof (SortedSetAttribute)))
                                instance = Activator.CreateInstance(typeof (SortedSet<>).MakeGenericType(itemType));
                        }
                        else
                            instance = Activator.CreateInstance(collectionType);

                        // for all the objects in the item type, add them
                        var method = collectionType.GetMethod("Add");
                        foreach (var value in values)
                        {
                            method.Invoke(instance, new [] {value});
                        }
                    }
                }

                return result;
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
