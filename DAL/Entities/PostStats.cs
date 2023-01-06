using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class PostStats
    {
        public Guid Id { get; set; }
        public long CommentsAmount { get; set; }
        public long LikesAmount { get; set; }
    }

    public class PostStatsPersonal : PostStats
    {
        public DateTimeOffset? WhenLiked { get; set; }
    }
}
