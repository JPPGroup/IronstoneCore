using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using System.IO;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Core.UI.Debug
{
    public class DebugViewModel
    {
        public DebugViewModel()
        {
            NamedObjects = new List<string>();
        }

        public List<string> NamedObjects { get; set; }

        public void Scan()
        {
            NamedObjects.Clear();

            var currentDocument = Application.DocumentManager.MdiActiveDocument;
            using (Transaction tr = currentDocument.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(currentDocument.Database.NamedObjectsDictionaryId, OpenMode.ForRead);

                foreach (var entry in nod)
                {
                    if (entry.Key.StartsWith("Jpp.Ironstone"))
                    {
                        NamedObjects.Add(entry.Key);
                    }
                }
            }
        }

        public void CopyDataToClipboard(string id)
        {
            var currentDocument = Application.DocumentManager.MdiActiveDocument;
            using (Transaction tr = currentDocument.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(currentDocument.Database.NamedObjectsDictionaryId,
                    OpenMode.ForRead);
                if (nod.Contains(id))
                {
                    ObjectId plotId = nod.GetAt(id);
                    Xrecord plotXRecord = (Xrecord)tr.GetObject(plotId, OpenMode.ForRead);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        foreach (TypedValue value in plotXRecord.Data)
                        {
                            byte[] data = new byte[512];

                            string message = (string)value.Value;
                            data = Encoding.ASCII.GetBytes(message);
                            ms.Write(data, 0, data.Length);
                        }

                        ms.Position = 0;

                        string s = Encoding.ASCII.GetString(ms.ToArray());
                        System.Windows.Clipboard.SetText(s);
                    }

                }
            }
        }
    }
}
