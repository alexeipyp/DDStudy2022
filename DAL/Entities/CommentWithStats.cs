using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CommentWithStats : Comment
    {
        public CommentStatsPersonal Stats { get; set; } = null!;
    }
}
