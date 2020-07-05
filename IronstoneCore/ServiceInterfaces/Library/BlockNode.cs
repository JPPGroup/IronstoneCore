using System;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Library
{
    public class BlockNode : LibraryNode
    {
        public Guid TemplateId { get; private set; }

        public BlockNode(string path, bool cacheDisabled, string name = null) : base(path, cacheDisabled, name)
        {
        }

        public override void Load()
        {
            throw new NotImplementedException();

        }

        public void Load(BlockDrawingObject blockDrawingObject)
        {
            TemplateId = new Guid(blockDrawingObject[TemplateDrawingObject.TEMPLATE_ID_KEY]);
        }
    }
}
