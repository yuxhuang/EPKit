using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPKit.ObjectGraph.Annotations
{
    public sealed class IdentifierAttribute : Attribute
    {
    }

    public sealed class IndexedAttribute : Attribute
    {
        public string Key { get; set; }
    }
}
