using Api.Models.Attachments;

namespace Api.Models.Post
{
    public class CreatePostModel
    {
        public Guid AuthorId { get; set; }
        public string? Annotation { get; set; }
        public List<MetaPathModel> Attaches { get; set; } = null!;
    }
}
