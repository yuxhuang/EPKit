using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPKit.ObjectGraph.Storage;

namespace EPKit.ObjectGraph
{
    interface IGraphContext
    {
        Task Persist<T>(T value);
    }
}
