﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions
{
    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException()
        {
        }

        public SessionNotFoundException(string message)
            : base(message)
        {
        }

        public SessionNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
