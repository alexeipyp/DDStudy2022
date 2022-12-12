using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CommentStats
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public long LikesAmount { get; set; }
    }
}
