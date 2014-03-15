using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace EPKit.ObjectGraph
{
    public interface IAsyncStorageProvider
    {
        IObjectStorage Object { get; }

        IHashStorage Hash { get; }

        ISetStorage Set { get; }

        ISortedSetStorage OrderSet { get; }
    }
}
