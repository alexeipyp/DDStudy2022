using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.NotFoundExceptions
{
    public class NotFoundException : Exception
    {
        public string? Model { get; set; }

        public override string Message => $"{Model} is not found";
        
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
