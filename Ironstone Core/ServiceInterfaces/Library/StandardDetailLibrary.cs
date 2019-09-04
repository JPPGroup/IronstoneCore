using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Newtonsoft.Json;


namespace Jpp.Ironstone.Core.ServiceInterfaces.Library
{
    class StandardDetailLibrary : ITemplateSource
    {
        private string _libraryRoot;
        private Dictionary<Guid, string> _templates;
        private string _cachePath;
        private bool _cacheDisabled = false;

        public ObservableCollection<LibraryNode> Nodes;

        public StandardDetailLibrary(IDataService dataService, IUserSettings settings)
        {
            dataService.RegisterSource(this);
            _libraryRoot = settings.GetValue("standarddetaillibrary.location");
            _cachePath = $"{_libraryRoot}\\Cache.json";
            string cacheSettingString = settings.GetValue("standarddetaillibrary.cachedisabled");
            if (!String.IsNullOrEmpty(cacheSettingString))
            {
                _cacheDisabled = bool.Parse(cacheSettingString);
            }

            Nodes = new ObservableCollection<LibraryNode>();

            LoadCache();
        }

        private void LoadCache()
        {
            if (File.Exists(_cachePath))
            {
                string json;
                using (StreamReader sr = File.OpenText(_cachePath))
                {
                    json = sr.ReadToEnd();
                }

                Nodes = JsonConvert.DeserializeObject<ObservableCollection<LibraryNode>>(json);
            }
            else
            {
                ParseForTemplates();
            }
        }

        private void ParseForTemplates()
        {
            foreach (string d in Directory.EnumerateDirectories(_libraryRoot))
            {
                LibraryNode ln = new LibraryNode()
                {
                    Name = Path.GetDirectoryName(d),
                    Path = d
                };

                ParseNode(ln);
                Nodes.Add(ln);
            }

            SaveCache();
        }

        private void ParseNode(LibraryNode node)
        {
            foreach (string d in Directory.EnumerateDirectories(node.Path))
            {
                LibraryNode ln = new LibraryNode()
                {
                    Name = new DirectoryInfo(Path.GetDirectoryName(d)).Name,
                    Path = d
                };

                ParseNode(ln);
                node.Children.Add(ln);
            }

            foreach (string s in Directory.EnumerateFiles(node.Path, "*.dwg"))
            {
                LibraryNode ln = new LibraryNode()
                {
                    Name = Path.GetFileNameWithoutExtension(s),
                    Path = s
                };

                LoadTemplatesFromNode(ln);
                node.Children.Add(ln);
            }
        }

        private void LoadTemplatesFromNode(LibraryNode node)
        {
            using (Database db = new Database(false, true))
            {
                db.ReadDwgFile(node.Path, FileOpenMode.OpenForReadAndAllShare, true, null);
                using (Transaction acTrans = db.TransactionManager.StartTransaction())
                {
                    var blocks = db.GetAllBlocks();
                    foreach (BlockTableRecord btr in blocks)
                    {
                        BlockDrawingObject bdo = new BlockDrawingObject(db);
                        bdo.BaseObject = btr.ObjectId;
                        if (bdo.HasKey(Constants.TEMPLATE_MASTER_KEY))
                        {
                            LibraryLeaf ln = new LibraryLeaf()
                            {
                                Name = btr.Name,
                                TemplateKey = Guid.Parse(bdo[Constants.TEMPLATE_MASTER_KEY])
                            };

                            node.Children.Add(ln);
                        }
                    }
                }
            }
        }

        private void SaveCache()
        {
            if (!_cacheDisabled)
            {
                string json;

                using (StreamWriter sw = File.CreateText(_cachePath))
                {
                    json = JsonConvert.SerializeObject(Nodes);
                    sw.Write(json);
                }
            }
        }

        public IEnumerable<Guid> GetAllTemplates()
        {
            if (_libraryRoot == null)
                return null;

            throw new NotImplementedException();
        }

        public DrawingObject GetTemplate(Guid id)
        {
            if (_libraryRoot == null)
                return null;

            throw new NotImplementedException();
        }

        public bool Contains(Guid id)
        {
            if (_libraryRoot == null)
                return false;

            throw new NotImplementedException();
        }
    }
}
