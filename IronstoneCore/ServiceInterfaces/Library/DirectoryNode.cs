using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Library
{
    public class DirectoryNode : LibraryNode
    {
        [JsonConstructor]
        public DirectoryNode(string path, bool cacheDisabled, string name = null) : base(path, cacheDisabled, name)
        {
        }

        internal DirectoryNode() : base()
        {

        }

        public override void Load()
        {
            if (Path.Equals("ProgramFiles", StringComparison.CurrentCultureIgnoreCase))
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Libraries");
            }

            if (!Directory.Exists(Path))
            {
                Status = NodeStatus.NotFound;
                return;
            }
            else
            {
                Status = NodeStatus.Loaded;
            }

            foreach(string s in Directory.GetDirectories(Path))
            {
                DirectoryNode node = new DirectoryNode(s, CacheDisabled);
                node.Load();
                Children.Add(node);
            }

            foreach (string file in Directory.GetFiles(Path))
            {
                if (System.IO.Path.GetExtension(file).EndsWith("dwg"))
                {
                    DrawingNode node = new DrawingNode(file, CacheDisabled);
                    node.Load();
                    Children.Add(node);
                }
            }
        }
    }
}
