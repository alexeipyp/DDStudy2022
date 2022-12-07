using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.UnauthorizedExceptions
{
    public class InvalidSessionException : UnauthorizedException
    {
        public override string Message => "Invalid session";

        public InvalidSessionException()
        {
        }

        public InvalidSessionException(string message)
            : base(message)
        {
        }

        public InvalidSessionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
