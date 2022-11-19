namespace Api.Models.Post
{
    public class CreateCommentPostModel
    {
        public Guid PostId { get; set; }
        public string Text { get; set; } = null!;
        public Guid AuthorId { get; set; }
    }
}
