using System;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class CompositeDrawingObject : DrawingObject
    {
        public ObservableCollection<DrawingObject> ChildObjects { get; set; }

        public override ObjectId BaseObject
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public CompositeDrawingObject() : base()
        {
            ChildObjects = new ObservableCollection<DrawingObject>();
            ChildObjects.CollectionChanged += ChildObjects_CollectionChanged;
        }

        private void ChildObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                /*foreach (DrawingObject newItem in e.NewItems)
                {
                    
                }*/
                DirtyModified = true;
            }

            if (e.OldItems != null)
            {
                /*foreach (DrawingObject oldItem in e.OldItems)
                {
                    
                }*/
                DirtyModified = true;
            }
        }

        public CompositeDrawingObject(Database database) : base(database)
        {
            ChildObjects = new ObservableCollection<DrawingObject>();
            ChildObjects.CollectionChanged += ChildObjects_CollectionChanged;
        }

        public override void CreateActiveObject()
        {
            foreach (DrawingObject drawingObject in ChildObjects)
            {
                drawingObject.CreateActiveObject();
            }
        }

        public override void Generate()
        {
            foreach (DrawingObject drawingObject in ChildObjects)
            {
                drawingObject.Generate();
            }
        }
    }
}
