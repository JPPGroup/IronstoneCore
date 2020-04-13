using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces.Template;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Library
{
    public class DrawingNode : LibraryNode, ITemplateSource
    {
        private HashSet<Guid> _containedTemplates;

        public DrawingNode(string path, bool cacheDisabled, string name = null) : base(path, cacheDisabled, name)
        {
        }

        public override void Load()
        {
            using (Database template = new Database())
            {
                template.ReadDwgFile(Path, FileOpenMode.OpenForReadAndAllShare, false, null);
                template.CloseInput(true);

                using (Transaction trans = template.TransactionManager.StartTransaction())
                {
                    IEnumerable<BlockTableRecord> definitions = template.GetAllBlockDefinitions();
                    foreach (BlockTableRecord blockTableRecord in definitions)
                    {
                        BlockDrawingObject blockDrawingObject = new BlockDrawingObject(template);
                        blockDrawingObject.BaseObject = blockTableRecord.ObjectId;
                        if (blockDrawingObject.HasKey(TemplateDrawingObject.TEMPLATE_ID_KEY))
                        {
                            BlockNode newNode = new BlockNode(this.Path, CacheDisabled, blockDrawingObject.Name);
                            newNode.Load(blockDrawingObject);
                            this.Children.Add(newNode);
                        }
                    }
                }
            }
        }

        public IEnumerable<Guid> GetAllTemplates()
        {
            return _containedTemplates;
        }

        public BlockDrawingObject GetTemplate(Guid id)
        {
            if (Contains(id))
            {
                // TODO: Implement
                return null;
            }
            else
            {
                throw new InvalidOperationException("Key not recognised");
            }
        }

        public bool Contains(Guid id)
        {
            return _containedTemplates.Contains(id);
        }
    }
}
