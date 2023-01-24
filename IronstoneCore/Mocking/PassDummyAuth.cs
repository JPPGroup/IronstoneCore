using System.ComponentModel;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.Mocking
{
    class PassDummyAuth : IAuthentication
    {
        public event PropertyChangedEventHandler PropertyChanged { add { } remove { }}

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
