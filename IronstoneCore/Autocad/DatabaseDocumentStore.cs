using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    /// <summary>
    /// Lightweight version of a document store for 
    /// </summary>
    public class DatabaseDocumentStore
    {
        public List<IDrawingObjectManager> Managers { get; private set; }
        private readonly Type[] _managerTypes;

        protected Database AcCurDb;

        public DatabaseDocumentStore(Database database)
        {
            AcCurDb = database;
        }

        /*internal void LoadWrapper()
        {
            try
            {
                using (Transaction tr = AcCurDb.TransactionManager.StartTransaction())
                {
                    Managers.Clear();

                    var mgrObjList = LoadBinary<List<object>>("Managers", _managerTypes);

                    foreach (IDrawingObjectManager drawingObjectManager in mgrObjList)
                    {
                        //drawingObjectManager.SetDependencies(AcDoc, _log);
                        //drawingObjectManager.ActivateObjects();
                        Managers.Add(drawingObjectManager);
                    }

                    tr.Commit();
                }

            }
            catch (Exception e)
            {
                _log.LogException(e);
            }
        }


        protected T LoadBinary<T>(string key, Type[] additionalTypes = null) where T : new()
        {
            //Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = AcCurDb.TransactionManager.TopTransaction;

            // Find the NOD in the database
            DBDictionary nod = (DBDictionary)tr.GetObject(AcCurDb.NamedObjectsDictionaryId, OpenMode.ForWrite);

            string id = this.GetType().FullName + key;

            if (nod.Contains(id))
            {
                ObjectId plotId = nod.GetAt(id);
                Xrecord plotXRecord = (Xrecord)tr.GetObject(plotId, OpenMode.ForRead);
                MemoryStream ms = new MemoryStream();
                foreach (TypedValue value in plotXRecord.Data)
                {
                    byte[] data = new byte[512];

                    string message = (string)value.Value;
                    data = Encoding.ASCII.GetBytes(message);
                    ms.Write(data, 0, data.Length);
                }
                ms.Position = 0;

                XmlSerializer xml;

                if (additionalTypes == null)
                {
                    xml = new XmlSerializer(typeof(T));
                }
                else
                {
                    xml = new XmlSerializer(typeof(T), additionalTypes);
                }

                try
                {
                    string s = Encoding.ASCII.GetString(ms.ToArray());
                    return (T)xml.Deserialize(ms);
                }
                catch (Exception e)
                {
                    _log.LogException(e);
                    return new T();
                }
            }

            //TODO: check changing from default has not broken this
            return new T();
        }*/
    }
}
