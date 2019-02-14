using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Module = System.Reflection.Module;

namespace Jpp.Ironstone.Core.UI.ViewModels
{
    public class AboutViewModel : BaseNotify
    {
        public string Version { get; set; }

        public string AuthStatus
        {
            get { return _authStatus; }
            set { SetField(ref _authStatus, value, "AuthStatus"); }
        }

        private string _authStatus;

        public IEnumerable<Jpp.Ironstone.Core.ServiceInterfaces.Module> Modules
        {
            get { return _loader.GetModules(); }
        }

        private IModuleLoader _loader;
        private IAuthentication _authentication;

        public AboutViewModel(IModuleLoader loader, IAuthentication auth)
        {
            _loader = loader;
            _authentication = auth;
            _authentication.PropertyChanged += _authentication_PropertyChanged;
            AuthStatus = _authentication.Status.ToString();

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void _authentication_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AuthStatus = _authentication.Status.ToString();
        }
    }
}
