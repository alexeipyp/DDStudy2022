using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.ForbiddenExceptions
{
    public class SessionNotActiveException : ForbiddenException
    {
        public SessionNotActiveException()
        {
        }

        public SessionNotActiveException(string message)
            : base(message)
        {
        }

        public SessionNotActiveException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
