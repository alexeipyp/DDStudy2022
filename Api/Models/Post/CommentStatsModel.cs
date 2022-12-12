namespace Api.Models.Post
{
    public class CommentStatsModel
    {
        public long LikesAmount { get; set; }
        public bool IsLiked { get; set; } = false;
    }
}
