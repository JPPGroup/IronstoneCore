using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    internal class ReviewManager : IReviewManager
    {
        private Dictionary<string, Dictionary<long, DrawingObject>> _objectCollections;
        private IDataService _dataService;

        public Dictionary<long, DrawingObject> this[string s]
        {
            get
            {
                if (!_objectCollections.ContainsKey(s))
                    ParseDocument(s);

                return _objectCollections[s];
            }
        }

        public ReviewManager(IDataService dataService)
        {
            _dataService = dataService;
            _objectCollections = new Dictionary<string, Dictionary<long, DrawingObject>>();
        }

        private void ParseDocument(string s)
        {
            Dictionary<long, DrawingObject> objects = new Dictionary<long, DrawingObject>();

            foreach (Document d in Application.DocumentManager)
            {
                if (d.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                {
                    //Iterate through all stores, adding managed objects to collection
                    IEnumerable<DocumentStore> stores = _dataService.GetExistingStores(d.Name);
                    foreach (DocumentStore documentStore in stores)
                    {
                        foreach (IDrawingObjectManager manager in documentStore.Managers)
                        {
                            foreach (DrawingObject drawingObject in manager.GetAllDrawingObjects())
                            {
                                objects.Add(drawingObject.BaseObjectPtr, drawingObject);
                            }
                        }
                    }

                    if (_objectCollections.ContainsKey(s))
                    {
                        _objectCollections[s] = objects;
                    }
                    else
                    {
                        _objectCollections.Add(s, objects);
                    }

                    return;
                }
            }

            //No document found, throw exception
            throw new ArgumentOutOfRangeException("ReviewManager did not recognise document key");
        }

        public IEnumerable<KeyValuePair<long, DrawingObject>> GetUnverified(Document doc)
        {
            return this[doc.Name].Where(x => x.Value.Verified == false);
        }

        public void Verify(Document doc, long id)
        {
            var collection = this[doc.Name];
            if (!collection.ContainsKey(id))
                throw new ArgumentOutOfRangeException("Object id not recognised as one being managed");

            collection[id].Verified = true;
        }

        public void Refresh(Document doc)
        {
            ParseDocument(doc.Name);
        }

        public void FocusOn(Document doc, long id)
        {
            var collection = this[doc.Name];
            if (!collection.ContainsKey(id))
                throw new ArgumentOutOfRangeException("Object id not recognised as one being managed");

            // TODO: FIX THIS!!!!
            //ViewHelper.FocusBoundingInModel(collection[id].GetBoundingBox());
        }
    }
}
