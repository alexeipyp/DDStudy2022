﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CustomExceptions.NotFoundExceptions
{
    public class CommentNotFoundException : NotFoundException
    {
        public CommentNotFoundException()
        {
            Model = "Comment";
        }

        public CommentNotFoundException(string message)
            : base(message)
        {
        }

        public CommentNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
