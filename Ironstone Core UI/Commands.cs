using System;
using System.Diagnostics;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

[assembly: CommandClass(typeof(Commands))]
namespace Jpp.Ironstone.Core.UI
{
    public static class Commands
    {
        private static bool _handled;

        [CommandMethod("DEBUG_SHOW_HANDLE")]
        [IronstoneCommand]
        public static void SetShowHandle()
        {
            try
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
            catch (System.Exception e)
            {
                CoreUIExtensionApplication.Current.Logger.LogException(e);
                throw;
            }
        }

        [CommandMethod("Core_Feedback")]
        [IronstoneCommand]
        public static void OpenFeedbackPage()
        {
            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true, // true is the default, but it is important not to set it to false
                        FileName = "https://jppuk.atlassian.net/servicedesk/customer/portals"
                    }
                };

                process.Start();
            }
            catch (Exception e)
            {
                CoreUIExtensionApplication.Current.Logger.LogException(e);
            }
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
            e.AppendToolTipText($"{id.ObjectClass.Name}\nHandle Pointer : {id.Handle.Value}");
        }
    }
}
