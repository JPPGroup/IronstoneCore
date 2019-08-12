using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Library
{
    public class LibraryNode
    {
        public string Name { get; set; }
        public ObservableCollection<LibraryNode> Children { get; set; }
        public string Path { get; set; }

        public LibraryNode()
        {
            Children = new ObservableCollection<LibraryNode>();
        }
    }
}
