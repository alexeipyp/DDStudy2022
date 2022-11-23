using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class BlackListItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BlockedUserId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual User BlockedUser { get; set; } = null!;
    }
}
