using System.Collections.Generic;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.ServiceInterfaces.Library;

namespace Jpp.Ironstone.Core.UI.ViewModels
{
    class LibrariesViewModel
    {
        public IReadOnlyList<LibraryNode> RootLibraries { get; private set; }

        public LibrariesViewModel()
        {
            RootLibraries = DataService.Current.RootLibraries;
        }
    }
}
