using Api.Models.User;

namespace Api.Models.Post
{
    public class CommentModel
    {
        public string Text { get; set; } = null!;
        public DateTimeOffset UploadDate { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
    }
}
