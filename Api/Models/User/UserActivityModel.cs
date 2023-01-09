using Api.Models.Subscribes;

namespace Api.Models.User
{
    public class UserActivityModel
    {
        public long PostsAmount { get; set; }
        public long FollowersAmount { get; set; }
        public long FollowingAmount { get; set; }
        public SubscribeStatusModel SubscribeStatus { get; set; } = null!;
    }
}
