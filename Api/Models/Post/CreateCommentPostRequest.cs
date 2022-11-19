namespace Api.Models.Post
{
    public class CreateCommentPostRequest
    {
        public Guid PostId { get; set; }
        public string Text { get; set; } = null!;
    }
}
