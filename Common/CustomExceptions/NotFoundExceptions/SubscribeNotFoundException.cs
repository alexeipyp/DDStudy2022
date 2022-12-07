using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.NotFoundExceptions
{
    public class SubscribeNotFoundException : NotFoundException
    {
        public SubscribeNotFoundException()
        {
            Model = "Subscribe";
        }

        public SubscribeNotFoundException(string message)
            : base(message)
        {
        }

        public SubscribeNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
