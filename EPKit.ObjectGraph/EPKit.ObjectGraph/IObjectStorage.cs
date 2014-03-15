using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPKit.ObjectGraph
{
    public interface IObjectStorage
    {
        Task Set<T>(T value);
        Task Set<T>(IEnumerable<T> values);
        Task<T> Get<T>(string key);
        Task<IEnumerable<T>> Get<T>(IEnumerable<string> keys);
        Task<int> Increment(string key, int value = 1);
        Task<float> Increment(string key, float value);
    }
}