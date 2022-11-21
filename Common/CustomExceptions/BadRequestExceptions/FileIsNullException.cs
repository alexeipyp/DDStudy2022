using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.BadRequestExceptions
{
    public class FileIsNullException : BadRequestException
    {
        public FileIsNullException()
        {
        }

        public FileIsNullException(string message)
            : base(message)
        {
        }

        public FileIsNullException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
