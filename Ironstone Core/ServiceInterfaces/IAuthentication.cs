using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IAuthentication
    {
        bool Authenticated();

        bool AuthenticateModule(string Path);

        AuthStatus GetAuthStatus();
    }

    public enum AuthStatus
    {
        OK,
        NoDongle,
        Unknown
    }
}
