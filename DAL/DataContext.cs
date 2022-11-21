using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasIndex(f => f.Email)
                .IsUnique();
            modelBuilder
                .Entity<User>()
                .HasIndex(f => f.Name)
                .IsUnique();

            modelBuilder.Entity<LikeToPost>().HasKey(s => new { s.UserId, s.PostId });
            modelBuilder.Entity<LikeToComment>().HasKey(s => new { s.UserId, s.CommentId });

            modelBuilder.Entity<Subscribe>().ToTable(nameof(Subscribes), t =>
            {
                t.HasCheckConstraint("CK_Subscribes", "\"AuthorId\" <> \"FollowerId\"");
            });
            modelBuilder.Entity<Subscribe>().HasKey(s => new { s.AuthorId, s.FollowerId });
            modelBuilder.Entity<Subscribe>()
                .HasOne(s => s.Author)
                .WithMany(x => x.Followers)
                .HasForeignKey(s => s.AuthorId);
            modelBuilder.Entity<Subscribe>()
                .HasOne(s => s.Follower)
                .WithMany(x => x.Subscribes)
                .HasForeignKey(s => s.FollowerId);

            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
            modelBuilder.Entity<PostAttach>().ToTable(nameof(PostAttaches));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("Api"));

        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostAttach> PostAttaches => Set<PostAttach>();
        public DbSet<LikeToPost> LikesToPosts => Set<LikeToPost>();
        public DbSet<LikeToComment> LikesToComments => Set<LikeToComment>();
        public DbSet<Subscribe> Subscribes => Set<Subscribe>();
    }
}