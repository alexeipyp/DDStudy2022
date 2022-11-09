using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string? Annotation { get; set; }
        public DateTimeOffset UploadDate { get; set; }
        public Guid AuthorId { get; set; }

        public virtual User Author { get; set; } = null!;
        public virtual ICollection<PostAttach> PostAttaches { get; set; } = null!;
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
