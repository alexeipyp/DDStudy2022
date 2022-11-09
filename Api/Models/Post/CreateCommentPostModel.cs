namespace Api.Models.Post
{
    public class CreateCommentPostModel
    {
        public Guid PostId { get; set; }
        public string Text { get; set; } = null!;

        public Guid AuthorId { get; set; }

        public CreateCommentPostModel(CreateCommentPostRequestModel request, Guid authorId)
        {
            PostId = request.PostId;
            Text = request.Text;
            AuthorId = authorId;
        }
    }
}
