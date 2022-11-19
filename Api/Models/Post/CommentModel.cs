using Api.Models.User;

namespace Api.Models.Post
{
    public class CommentModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset UploadDate { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
        public long LikesAmount { get; set; }
        public bool IsLiked { get; set; } = false;

    }
}
