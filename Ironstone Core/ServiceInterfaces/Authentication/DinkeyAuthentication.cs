using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Authentication
{
    public class DinkeyAuthentication : IAuthentication
    {
        public int ErrorCode { get; set; } = -1;

        public bool Authenticated()
        {
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

        public AuthStatus GetAuthStatus()
        {
            switch (ErrorCode)
            {
                case 0:
                    return AuthStatus.OK;

                default:
                    return AuthStatus.Unknown;
            }
        }
    }
}
