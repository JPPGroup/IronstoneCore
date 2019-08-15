using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;

namespace Jpp.Ironstone.Core.UI.ViewModels.DatabaseExplorer
{
    public class DatabaseEntry
    {
        public Document Host { get; set; }

        public string Name { get; set; }

        public List<DatabaseEntry> Children { get; private set; }

        public DatabaseEntry()
        {
            Children = new List<DatabaseEntry>();
            
        }
    }
}
