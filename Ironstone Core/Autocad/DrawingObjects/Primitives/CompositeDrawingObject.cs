using System;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class CompositeDrawingObject<T> : DrawingObject where T : DrawingObject
    {
        public ObservableCollection<T> ChildObjects { get; set; }

        public override ObjectId BaseObject
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        protected CompositeDrawingObject() : base()
        {
            ChildObjects = new ObservableCollection<T>();
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

        protected CompositeDrawingObject(Database database) : base(database)
        {
            ChildObjects = new ObservableCollection<T>();
            ChildObjects.CollectionChanged += ChildObjects_CollectionChanged;
        }

        public override void CreateActiveObject()
        {
            foreach (T drawingObject in ChildObjects)
            {
                drawingObject.CreateActiveObject();
            }
        }

        public override void Generate()
        {
            foreach (T drawingObject in ChildObjects)
            {
                drawingObject.Generate();
            }
        }
    }
}
