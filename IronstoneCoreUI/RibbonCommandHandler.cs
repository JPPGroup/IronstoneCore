using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.UI
{
    class RibbonCommandHandler : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true; //return true means the button always enabled
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //TODO: Add authentication check here
            RibbonCommandItem cmd = parameter as RibbonCommandItem;
            Document dwg = Application.DocumentManager.MdiActiveDocument;
            if (cmd != null) dwg.SendStringToExecute((string)cmd.CommandParameter, true, false, false);
        }
    }
}
