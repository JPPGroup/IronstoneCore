using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.Autocad
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LayerAttribute : Attribute
    {
        public string Name;
    }
}
