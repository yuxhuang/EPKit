using EPKit.ObjectGraph.Storage;

namespace EPKit.ObjectGraph
{
    public interface IStorageContext
    {
        IObjectStorage Objects { get; }

        IHashStorage Hashes { get; }

        ISetStorage Sets { get; }

        ISortedSetStorage SortedSets { get; }
    }
}
