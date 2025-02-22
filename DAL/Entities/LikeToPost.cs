﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class LikeToPost : Like
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
