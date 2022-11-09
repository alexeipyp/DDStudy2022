using Api.Models.Attachments;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Api.Models.Post
{
    public class CreatePostRequestModel
    {
        public string? Annotation { get; set; }
        public List<MetadataModel> Attaches { get; set; } = null!;
    }
}
