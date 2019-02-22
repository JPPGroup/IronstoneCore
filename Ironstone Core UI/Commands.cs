using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(Jpp.Ironstone.Core.UI.Commands))]

namespace Jpp.Ironstone.Core.UI
{
    public class Commands
    {
        private bool _handled;

        [CommandMethod("SHOWHANDLE")]
        public void SetShowHandle()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var options = new PromptIntegerOptions("Enter new value for SHOWHANDLE: \n")
            {
                LowerLimit = 0,
                UpperLimit = 1,
                DefaultValue = Convert.ToInt32(_handled),
                UseDefaultValue = true
            };

            var result = ed.GetInteger(options);
            if (result.Status != PromptStatus.OK || Convert.ToBoolean(result.Value) == _handled) return;

            if (_handled)
            {
                ed.PointMonitor -= OnPointMonitor;
            }
            else
            {
                ed.PointMonitor += OnPointMonitor;
            }

            _handled = !_handled;
        }

        private static void OnPointMonitor(object sender, PointMonitorEventArgs e)
        {
            if ((e.Context.History & PointHistoryBits.FromKeyboard) == PointHistoryBits.FromKeyboard)
                return;

            var paths = e.Context.GetPickedEntities();

            if (paths == null || paths.Length == 0)
                return;

            var ids = paths[0].GetObjectIds();

            if (ids == null || ids.Length != 1)
                return;

            var id = ids[0];
            e.AppendToolTipText($"{id.ObjectClass.Name}\nHandle = {id.Handle}");
        }
    }
}
