using System;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Autocad
{
    public class LayerManager
    {
        private IUserSettings _settings;

        public LayerManager(IUserSettings settings)
        {
            _settings = settings;
        }

        public void CreateLayer(Database targetDatabase, string name)
        {
            LayerInfo newLayerInfo;

            if (_settings.GetValue($"{name}.name") != null)
            {
                newLayerInfo = new LayerInfo()
                {
                    IndexColor = short.Parse(_settings.GetValue($"{name}.color")),
                    LayerId = _settings.GetValue($"{name}.name"),
                    Linetype = _settings.GetValue($"{name}.linetype")
                };
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
            if (_settings.GetValue($"{name}.name") != null)
            {
                return _settings.GetValue($"{name}.name");
            }
            else
            {
                // TODO: Consider if this should cause an exception or if it should just log an error and set to layer 0
                throw new ArgumentNullException("Layer not found");
            }
        }
    }
}
