using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IAuthentication : INotifyPropertyChanged
    {
        AuthStatus Status { get; }

        bool Authenticated();

        bool AuthenticateModule(string Path);

        bool VerifyLicense(string LicenseName);
    }

    public enum AuthStatus
    {
        OK,
        DongleError,
        LicenseNotFound,
        NetworkLicenseFailure,
        Unknown
    }
}
