using System.Collections.ObjectModel;
using System.Globalization;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Library
{
    public abstract class LibraryNode
    {
        public string Name { get; protected set; }
        public ObservableCollection<LibraryNode> Children { get; }
        public string Path { get; protected set; }
        public bool PreloadDisabled { get; }
        public bool CacheDisabled { get; }

        public NodeStatus Status { get; protected set; } = NodeStatus.Unloaded;

        private LibraryNode()
        {
            Children = new ObservableCollection<LibraryNode>();
        }

        public LibraryNode(string path, bool cacheDisabled, string name = null) : this()
        {
            Path = path;
            if (name == null)
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                TextInfo textFormatter = new CultureInfo("en",false).TextInfo;
                Name = textFormatter.ToTitleCase(name);
            }
            CacheDisabled = cacheDisabled;
        }

        public abstract void Load();
    }

    public enum NodeStatus
    {
        Cached,
        Loaded,
        Unloaded,
        NotFound
    }
}
