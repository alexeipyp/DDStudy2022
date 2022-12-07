using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class UserActivity
    {
        public Guid Id { get; set; }
        public long PostsAmount { get; set; }
        public long FollowersAmount { get; set; }
        public long FollowingAmount { get; set; }
    }
}
