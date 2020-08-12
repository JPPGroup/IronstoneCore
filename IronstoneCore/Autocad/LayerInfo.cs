using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.Autocad
{
    public class LayerInfo
    {
        public string LayerId { get; set; }
        public short IndexColor { get; set; } = 7;
        public string Linetype { get; set; } = "CONTINUOUS";
    }
}
