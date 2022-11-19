using Api.Models.Attachments;
using Api.Models.User;
using DAL.Entities;

namespace Api.Models.Post
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public string? Annotation { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
        public List<AttachWithLinkModel>? Attaches { get; set; } = new List<AttachWithLinkModel>();
        public DateTimeOffset UploadDate { get; set; }
        public long CommentsAmount { get; set; }
        public long LikesAmount { get; set; }
        public bool IsLiked { get; set; }
    }
}
