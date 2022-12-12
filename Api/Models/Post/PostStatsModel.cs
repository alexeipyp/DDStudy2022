namespace Api.Models.Post
{
    public class PostStatsModel
    {
        public long CommentsAmount { get; set; }
        public long LikesAmount { get; set; }
        public bool IsLiked { get; set; }
    }
}
