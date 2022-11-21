using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.NotFoundExceptions
{
    public class PostNotFoundException : NotFoundException
    {
        public PostNotFoundException()
        {
        }

        public PostNotFoundException(string message)
            : base(message)
        {
        }

        public PostNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
