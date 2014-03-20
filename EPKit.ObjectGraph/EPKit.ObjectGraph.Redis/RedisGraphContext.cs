using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            using (var tx = _client.CreateTransaction())
            {
                var id = obj.GetIdentifier();
                
                // get all graph properties
                var props = typeof (T).GetGraphProperties(typeof (GraphPropertyAttribute));
                
                foreach (var prop in props)
                {
                    var property = prop;
                    var propertyValue = property.GetValue(obj);

                    if (propertyValue != null)
                    {
                        if (prop.IsGraphProperty(typeof (AssociatedAttribute)))
                        {
                            var propertyValueId = propertyValue.GetIdentifier();
                            tx.QueueCommand(c => c.Set(GetKey(typeof (T), id, property.Name), propertyValueId));
                            property.SetValue(obj, null);
                        }

                        if (prop.IsGraphProperty(typeof (SetAttribute)))
                        {
                            var key = GetKey(typeof (T), id, property.Name);
                            var set = (ISet<object>) propertyValue;
                            var setIds = set.Select(i => i.GetIdentifier()).ToList();
                            tx.QueueCommand(c => c.Sets[key].Clear());
                            foreach (var valueId in setIds)
                            {
                                var id1 = valueId;
                                tx.QueueCommand(c => c.Sets[key].Add(id1));
                            }
                            property.SetValue(obj, null);
                        }

                        if (prop.IsGraphProperty(typeof (SortedSetAttribute)) || prop.IsGraphProperty(typeof (CollectionAttribute)))
                        {
                            var key = GetKey(typeof (T), id, property.Name);
                            var list = (ICollection<object>) propertyValue;
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

                        if (prop.IsGraphProperty(typeof (HashAttribute)))
                        {
                            var key = GetKey(typeof (T), id, property.Name);
                            tx.QueueCommand(c => c.Hashes[key].Clear());
                            property.SetValue(obj, null);
                        }
                    }
                }

                tx.QueueCommand(c => c.Set(GetKey(typeof(T), id), obj));
                tx.Commit();
            }
        }

        public Task Remove<T>(T value) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Task RemoveById<T>(string id) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetById<T>(string id, params Expression<Func<T, object>>[] includes) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
