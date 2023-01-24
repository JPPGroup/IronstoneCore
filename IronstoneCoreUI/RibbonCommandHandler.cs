using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.UI
{
    class RibbonCommandHandler : ICommand
    {
        private Func<bool> _canExecute;
        
        public RibbonCommandHandler(Func<bool> execute)
        {
            _canExecute = execute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke();
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public void Execute(object parameter)
        {
            //TODO: Add authentication check here
            RibbonCommandItem cmd = parameter as RibbonCommandItem;
            Document dwg = Application.DocumentManager.MdiActiveDocument;
            if (cmd != null) dwg.SendStringToExecute((string)cmd.CommandParameter, true, false, false);
        }
    }
}
