using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Authentication
{
    public class DinkeyAuthentication : BaseNotify, IAuthentication
    {
        private int ErrorCode
        {
            get { return _errorCode; }
            set
            {
                _errorCode = value;
                Status = TranslateCode(value);
            }
        } 

        private int _errorCode;

        public AuthStatus Status
        {
            get { return _status; }
            private set { SetField(ref _status, value, "Status"); }
        }

        private AuthStatus _status;

        public DinkeyAuthentication()
        {
            ErrorCode = -1;
        }

        public bool Authenticated()
        {
#if DEBUG
            return true;
#endif

            ErrorCode = -1;

            int ret_code;
            DRIS dris = new DRIS(); // initialise the DRIS with random values & set the header

            dris.size = Marshal.SizeOf(dris);
            dris.function = DRIS.PROTECTION_CHECK; // standard protection check
            dris.flags = 0; // no extra flags, but you may want to specify some if you want to start a network user or decrement execs,...

            ErrorCode = DinkeyPro.DDProtCheck(dris, null);

            if (ErrorCode != 0)
            {
                //DisplayError(ret_code, dris.ext_err);
                return false;
            }

            return true;
        }

        public bool AuthenticateModule(string Path)
        {
            return true;
        }

        private AuthStatus TranslateCode(int Code)
        {
            switch (ErrorCode)
            {
                case 0:
                    return AuthStatus.OK;

                case 401://No Dongle found
                case 402: //To many dongles
                case 403: //Wrong type
                case 404: //Wrong model
                case 405: //Demo dongle
                case 406: //Demo software
                case 407: //Wrong company
                case 408: //Wrong serial
                case 409: //Not prgorammed
                    return AuthStatus.DongleError;

                case 410: //Missing prodcut code
                case 411: //Missing licesne
                case 413: //Program not protected
                case 419: //Clock tamper
                case 439: //Missing prduct code over network
                case 440: //missing license over network
                    return AuthStatus.LicenseNotFound;

                case 435: //No server found
                case 436: //No longer server conncted
                case 437: //Server update needed
                    return AuthStatus.NetworkLicenseFailure;

                default:
                    return AuthStatus.Unknown;
            }
        }
    }
}
