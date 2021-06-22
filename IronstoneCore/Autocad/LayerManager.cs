using System;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Configuration;

namespace Jpp.Ironstone.Core.Autocad
{
    public class LayerManager
    {
        private IConfiguration _settings;

        public LayerManager(IConfiguration settings)
        {
            _settings = settings;
        }

        public void CreateLayer(Database targetDatabase, string name)
        {
            LayerInfo newLayerInfo;

            if (_settings[$"{name}:name"] != null)
            {
                newLayerInfo = new LayerInfo()
                {
                    IndexColor = short.Parse(_settings[$"{name}:color"]),
                    LayerId = _settings[$"{name}:name"],
                    Linetype = "Continuous"
                };

                //TODO: Add unit test to confirm that the fallback works
                string linetype = _settings[$"{name}:linetype"];
                if (VerifyLinetype(targetDatabase, linetype))
                    newLayerInfo.Linetype = linetype;
            }
            else
            {
                newLayerInfo = new LayerInfo()
                {
                    LayerId = name
                };
            }

            targetDatabase.RegisterLayer(newLayerInfo);
        }

        public string GetLayerName(string name)
        {
            if (_settings[$"{name}:name"] != null)
            {
                return _settings[$"{name}:name"];
            }
            else
            {
                // TODO: Consider if this should cause an exception or if it should just log an error and set to layer 0
                throw new ArgumentNullException("Layer not found");
            }
        }

        private bool VerifyLinetype(Database targetDatabase, string linetype)
        {
            Transaction trans = targetDatabase.TransactionManager.TopTransaction;

            LinetypeTable acLineTypTbl;
            acLineTypTbl = trans.GetObject(targetDatabase.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

            return acLineTypTbl.Has(linetype);
        }
    }
}
