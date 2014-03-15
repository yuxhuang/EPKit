using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPKit.ObjectGraph.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ReferenceAttribute : Attribute
    {
        public string Name { get; set; }

        public ReferenceAttribute(string name = null)
        {
            Name = name;
        }
    }
}
