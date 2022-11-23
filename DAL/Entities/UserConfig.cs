﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class UserConfig
    {
        public Guid UserId { get; set; }
        public bool IsPrivate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
