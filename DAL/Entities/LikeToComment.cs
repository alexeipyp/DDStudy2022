using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class LikeToComment
    {
        public Guid UserId { get; set; }
        public Guid CommentId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Comment Comment { get; set; } = null!;
    }
}
