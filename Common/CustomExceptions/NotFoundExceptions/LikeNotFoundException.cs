using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.NotFoundExceptions
{
    public class LikeNotFoundException : NotFoundException
    {
        public LikeNotFoundException()
        {
            Model = "Like";
        }

        public LikeNotFoundException(string message)
            : base(message)
        {
        }

        public LikeNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
