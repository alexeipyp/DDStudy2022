namespace Api.Models.Attachments
{
    public class PostAttachModel
    {
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long Size { get; set; }
        public Guid AuthorId { get; set; }

        public PostAttachModel(MetadataModel model, Guid authorId, Func<MetadataModel, string> pathgen)
        {
            Name = model.Name;
            MimeType = model.MimeType;
            Size = model.Size;
            AuthorId = authorId;
            FilePath = pathgen(model);
        }
    }
}
