namespace Api.Models.Attachments
{
    public class MetaPathModel
    {
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long Size { get; set; }
        public Guid AuthorId { get; set; }

    }
}
