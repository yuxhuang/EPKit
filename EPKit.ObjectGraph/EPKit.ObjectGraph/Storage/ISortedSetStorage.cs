using System.Threading.Tasks;

namespace EPKit.ObjectGraph.Storage
{
    public interface ISortedSetStorage
    {
        Task Add(string key, string member, float score);
        Task Remove(string key, string member);
        Task<string[]> Get(string key, int start, int stop);
    }
}