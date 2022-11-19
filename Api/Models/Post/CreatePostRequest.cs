using Api.Models.Attachments;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Api.Models.Post
{
    public class CreatePostRequest
    {
        public string? Annotation { get; set; }
        public List<MetadataModel> Attaches { get; set; } = null!;
    }

    public class CreatePostAuthorizedRequest : CreatePostRequest
    {
        public Guid AuthorId { get; set; }

    }
}
