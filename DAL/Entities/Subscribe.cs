using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Subscribe
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public Guid FollowerId { get; set; }
        public bool IsAccepted { get; set; } = true;

        public virtual User Author { get; set; } = null!;
        public virtual User Follower { get; set; } = null!;
    }
}
