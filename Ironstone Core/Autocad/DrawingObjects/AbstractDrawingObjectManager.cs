using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.ServiceInterfaces;

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
        public IReadOnlyList<T> ActiveObjects {
            get
            {
                if(_activeObjects == null) SetActiveObjects();
                return _activeObjects;
            }
        }
        private IReadOnlyList<T> _activeObjects;

        [XmlIgnore]
        public Document HostDocument { get; private set; }
        [XmlIgnore]
        public ILogger Log { get; private set; }

        /// <summary>
        /// Create an instance of the manager
        /// </summary>
        /// <param name="document">The document in which the manager resides</param>
        /// <param name="log"></param>
        protected AbstractDrawingObjectManager(Document document, ILogger log)
        {
            HostDocument = document;
            ManagedObjects = new List<T>();
            Log = log;
        }

        protected AbstractDrawingObjectManager() { }

        public virtual void UpdateDirty()
        {
            //TODO: Review how to handle erased base objects
            //RemoveErased();

            SetActiveObjects();
        }


        public virtual void UpdateAll()
        {
            //TODO: Review how to handle erased base objects
            //RemoveErased();

            SetActiveObjects();
        }

        public void Clear()
        {
            foreach (T managedObject in ManagedObjects)
            {
                managedObject.Erase();
            }
            ManagedObjects.Clear();
            SetActiveObjects();
        }

        public virtual void AllDirty()
        {
            foreach (T managedObject in ManagedObjects)
            {
                managedObject.DirtyModified = true;
            }
        }

        public virtual void ActivateObjects()
        {
            //TODO: Trace usage paths to confirm transaction is needed
            using (Transaction acTrans = HostDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (T managedObject in ManagedObjects)
                {
                    managedObject.CreateActiveObject();
                }

                acTrans.Commit();
            }
        }

        /// <summary>
        /// Add new object to managed object collection
        /// </summary>
        /// <param name="toBeManaged">Object to be added. Dirty added does not need to be marked</param>
        public void Add(T toBeManaged)
        {
            if (!toBeManaged.Active) throw new ArgumentException("Drawing object not active.");

            ManagedObjects.Add(toBeManaged);
            toBeManaged.DirtyAdded = true;

            //TODO: Belt and Braces. Review implementation of Active and Managed object lists in line with dirty system.
            SetActiveObjects();
        }

        /// <summary>
        /// Method to iterate over managed objects and remove any that have been deleted
        /// </summary>
        public void RemoveErased()
        {
            //TODO: Review how to handle erased base objects
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

        public void SetDependencies(Document doc, ILogger log)
        {
            HostDocument = doc;
            Log = log;
        }

        public object[] GetRequiredLayers()
        {
            List<object> result = new List<object>();

            var t = this.GetType();
            var add = t.GetCustomAttributes(typeof(LayerAttribute), true);

            result.AddRange(add);
            result.AddRange(typeof(T).GetCustomAttributes(typeof(LayerAttribute), true));
            return result.ToArray();
        }


        private void SetActiveObjects()
        {
            _activeObjects = ManagedObjects.Where(obj => !obj.Erased).ToList().AsReadOnly();
        }
    }
}
