using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.Mocking
{
    class PassDummyAuth : IAuthentication
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PassDummyAuth(ILogger<IAuthentication> logger)
        {

        }

        public AuthStatus Status
        {
            get { return AuthStatus.OK; }
        }

        public bool Authenticated()
        {
            return true;
        }

        public bool AuthenticateModule(string Path)
        {
            return true;
        }

        public bool VerifyLicense(string LicenseName)
        {
            return true;
        }
    }
}
