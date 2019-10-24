using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class DrawingObjectBuilder<T> where T : DrawingObject
    {
        public IEnumerable<T> Build(ObjectIdCollection objectIdCollection)
        {
            var taggedList = new List<TaggedObjectId>();

            for (var i = 0; i < objectIdCollection.Count; i++)
            {
                var item = new TaggedObjectId { ObjectId = objectIdCollection[i], Tag = i };

                if (taggedList.Count == 0)
                {
                    taggedList.Add(item);
                    continue;
                }

                foreach (var tag in taggedList)
                {
                    if (tag.Tag == item.Tag) continue;
                    if (!IsConnected(tag.ObjectId, item.ObjectId)) continue;

                    if (item.Tag != i)
                    {
                        foreach (var tagged in taggedList.Where(obj => obj.Tag == item.Tag))
                        {
                            tagged.Tag = tag.Tag;
                        }
                    }

                    item.Tag = tag.Tag;
                }

                taggedList.Add(item);
            }

            return taggedList.GroupBy(c => c.Tag).Select(obj => CreateDrawingObject(obj.Select(e => e.ObjectId))).ToList();
        }

        protected abstract T CreateDrawingObject(IEnumerable<ObjectId> objectIds);

        protected abstract bool IsConnected(ObjectId firstObjectId, ObjectId secondObjectId);

        private class TaggedObjectId
        {
            public int Tag { get; set; }
            public ObjectId ObjectId { get; set; }
        }
    }
}
