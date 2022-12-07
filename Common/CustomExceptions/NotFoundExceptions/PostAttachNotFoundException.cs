using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.NotFoundExceptions
{
    public class PostAttachNotFoundException : NotFoundException
    {
        public PostAttachNotFoundException()
        {
            Model = "Post attach";
        }

        public PostAttachNotFoundException(string message)
            : base(message)
        {
        }

        public PostAttachNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
