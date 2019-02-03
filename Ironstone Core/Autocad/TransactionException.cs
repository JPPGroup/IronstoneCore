using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.Autocad
{
    class TransactionException : Exception
    {
        public TransactionException()
        {
        }

        public TransactionException(string message)
            : base(message)
        {
        }
    }
}
