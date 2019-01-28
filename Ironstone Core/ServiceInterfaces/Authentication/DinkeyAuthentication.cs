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
        public bool Authenticated()
        {
#if DEBUG
            return true;
#endif
            int ret_code;
            DRIS dris = new DRIS(); // initialise the DRIS with random values & set the header

            dris.size = Marshal.SizeOf(dris);
            dris.function = DRIS.PROTECTION_CHECK; // standard protection check
            dris.flags = 0; // no extra flags, but you may want to specify some if you want to start a network user or decrement execs,...

            ret_code = DinkeyPro.DDProtCheck(dris, null);

            if (ret_code != 0)
            {
                //DisplayError(ret_code, dris.ext_err);
                return false;
            }

            return true;
        }
    }
}
