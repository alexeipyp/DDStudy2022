using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class MuteListItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MutedUserId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual User MutedUser { get; set; } = null!;
    }
}
