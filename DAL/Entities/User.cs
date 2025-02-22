﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "empty";
        public string Email { get; set; } = "epmty";
        public string PasswordHash { get; set; } = "empty";
        public DateTimeOffset BirthDate { get; set; }

        public virtual Avatar? Avatar { get; set; }
        public virtual UserConfig Config { get; set; } = null!;
        public virtual ICollection<UserSession>? Sessions { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<LikeToPost>? LikedPosts { get; set; }
        public virtual ICollection<Subscribe>? Followers { get; set; }
        public virtual ICollection<Subscribe>? Subscribes { get; set; }
        public virtual ICollection<BlackListItem>? BlockedUsers { get; set; }
        public virtual ICollection<BlackListItem>? BlockedByUsers { get; set; }
        public virtual ICollection<MuteListItem>? MutedUsers { get; set; }
        public virtual ICollection<MuteListItem>? MutedByUsers { get; set; }
    }
}
