using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions
{
    public class UserCreationException : Exception
    {
        public UserCreationException()
        {
        }

        public UserCreationException(string message)
            : base(message)
        {
        }

        public UserCreationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
