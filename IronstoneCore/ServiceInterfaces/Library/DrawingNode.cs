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
            _containedTemplates = new HashSet<Guid>();
            DataService.Current.RegisterSource(this); // TODO: Add check for registering
        }

        // TODO: Add tests
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
                            _containedTemplates.Add(newNode.TemplateId);
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

        // TODO: Add tests
        public (Database, TemplateDrawingObject) GetTemplate(Guid id)
        {
            if (Contains(id))
            {
                Database template = new Database();

                template.ReadDwgFile(Path, FileOpenMode.OpenForReadAndAllShare, false, null);
                template.CloseInput(true);

                using (Transaction trans = template.TransactionManager.StartTransaction())
                {
                    IEnumerable<BlockTableRecord> definitions = template.GetAllBlockDefinitions();
                    foreach (BlockTableRecord blockTableRecord in definitions)
                    {
                        BlockDrawingObject blockDrawingObject = new BlockDrawingObject(template);
                        blockDrawingObject.BaseObject = blockTableRecord.ObjectId;
                        if (blockDrawingObject.HasKey(TemplateDrawingObject.TEMPLATE_ID_KEY) && new Guid(blockDrawingObject[TemplateDrawingObject.TEMPLATE_ID_KEY]) == id)
                        {
                            TemplateDrawingObject templateObject = new TemplateDrawingObject(template);
                            templateObject.BaseObject = blockDrawingObject.BaseObject;
                            return (template, templateObject);
                        }
                    }
                }

                throw new InvalidOperationException("Key not found");
            }
            else
            {
                throw new InvalidOperationException("Key not recognised");
            }
        }

        // TODO: Add tests
        public bool Contains(Guid id)
        {
            return _containedTemplates.Contains(id);
        }
    }
}
