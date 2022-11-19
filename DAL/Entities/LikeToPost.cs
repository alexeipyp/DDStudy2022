using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class LikeToPost
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
    }
}
