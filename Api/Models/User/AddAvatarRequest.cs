using Api.Models.Attachments;

namespace Api.Models.User
{
    public class AddAvatarRequest : MetadataModel
    {
        public Guid? AuthorId { get; set; }
    }
}
