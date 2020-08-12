using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    internal class LayerState
    {
        public string Name { get;  }
        public bool IsOff { get;  }
        public bool IsFrozen { get; }
        public bool IsLocked { get; }
        public bool IsInvalid => IsFrozen || IsLocked || IsOff;

        public LayerState(LayerTableRecord layer)
        {
            Name = layer.Name;
            IsOff = layer.IsOff;
            IsFrozen = layer.IsFrozen;
            IsLocked = layer.IsLocked;
        }

        
    }
}
