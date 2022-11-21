using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.ForbiddenExceptions
{
    public class NotSubscribedException : ForbiddenException
    {
        public NotSubscribedException()
        {
        }

        public NotSubscribedException(string message)
            : base(message)
        {
        }

        public NotSubscribedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
