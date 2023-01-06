using DAL.Entities;

namespace Api.Models.Post
{
    public class CommentStatsModel
    {
        public long LikesAmount { get; set; }
        public DateTimeOffset? WhenLiked { get; set; }
    }
}
