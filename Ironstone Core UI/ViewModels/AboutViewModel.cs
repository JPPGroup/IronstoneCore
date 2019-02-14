using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Module = System.Reflection.Module;

namespace Jpp.Ironstone.Core.UI.ViewModels
{
    public class AboutViewModel
    {
        public string Version { get; set; }

        public string AuthStatus
        {
            get
            {
                return _authentication.GetAuthStatus().ToString();
            }
        }

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

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
