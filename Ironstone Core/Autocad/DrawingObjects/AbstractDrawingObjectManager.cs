using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    /// <summary>
    /// Abstract manager implementation for Drawing Objects.
    /// </summary>
    /// <typeparam name="T">Class that extends DrawingObject</typeparam>
    public abstract class AbstractDrawingObjectManager<T> : IDrawingObjectManager where T : DrawingObject
    {
        public List<T> ManagedObjects;

        [XmlIgnore]
        public Document HostDocument { get; set; }

        /// <summary>
        /// Create an instance of the manager
        /// </summary>
        /// <param name="document">The document in which the manager resides</param>
        public AbstractDrawingObjectManager(Document document)
        {
            HostDocument = document;
            ManagedObjects = new List<T>();
        }

        protected AbstractDrawingObjectManager()
        {
        }

        public virtual void UpdateDirty()
        {
            RemoveErased();
        }

        public abstract void UpdateAll();

        public abstract void Clear();

        public virtual void AllDirty()
        {
            foreach (T managedObject in ManagedObjects)
            {
                managedObject.DirtyModified = true;
            }
        }

        public virtual void ActivateObjects()
        {
            // Get the current document and database
            Database acCurDb = HostDocument.Database;

            //TODO: Trace usage paths to confirm transaction is needed
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (T managedObject in ManagedObjects)
                {
                    managedObject.CreateActiveObject();
                }
            }
        }

        /// <summary>
        /// Add new object to managed object collection
        /// </summary>
        /// <param name="toBeManaged">Object to be added. Dirty added does not need to be marked</param>
        public void Add(T toBeManaged)
        {
            ManagedObjects.Add(toBeManaged);
            toBeManaged.DirtyAdded = true;
        }

        /// <summary>
        /// Method to iterate over managed objects and remove any that have been deleted
        /// </summary>
        public void RemoveErased()
        {
            List<int> indicesToRemove = new List<int>();

            for (int i = ManagedObjects.Count - 1; i >= 0; i--)
            {
                if (ManagedObjects[i].DirtyRemoved)
                {
                    indicesToRemove.Add(i);
                }
            }

            indicesToRemove.Reverse();

            foreach (int i in indicesToRemove)
            {
                ManagedObjects.RemoveAt(i);
            }
        }
    }
}
