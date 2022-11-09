namespace Api.Models.Post
{
    public class CreateCommentPostRequestModel
    {
        public Guid PostId { get; set; }
        public string Text { get; set; } = null!;
    }
}
