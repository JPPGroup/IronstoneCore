using System;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Autocad
{
    public class LayerManager
    {
        private IUserSettings _settings;
        private ILogger _logger;

        public LayerManager(IUserSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void CreateLayer(Database targetDatabase, string name)
        {
            LayerInfo newLayerInfo;

            if (_settings.GetValue($"layers.{name}.name") != null)
            {
                newLayerInfo = new LayerInfo()
                {
                    IndexColor = short.Parse(_settings.GetValue($"layers.{name}.color")),
                    LayerId = _settings.GetValue($"layers.{name}.name"),
                    Linetype = _settings.GetValue($"layers.{name}.linetype")
                };
            }
            else
            {
                newLayerInfo = new LayerInfo()
                {
                    LayerId = name
                };
            }

            try
            {
                targetDatabase.RegisterLayer(newLayerInfo);
            }
            catch (ArgumentOutOfRangeException)
            {
                // TODO: Test this fails with a made up layer linetype, and correctly goes to defaults

                _logger.Entry($"Invalid layer settings for {name}, using defaults.");
                targetDatabase.RegisterLayer(newLayerInfo.LayerId);
            }
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
