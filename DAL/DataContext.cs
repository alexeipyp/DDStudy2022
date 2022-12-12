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
            modelBuilder.Ignore<PostWithStats>();
            modelBuilder.Ignore<CommentWithStats>();

            modelBuilder
                .Entity<User>()
                .HasIndex(f => f.Email)
                .IsUnique();
            modelBuilder
                .Entity<User>()
                .HasIndex(f => f.Name)
                .IsUnique();
            modelBuilder
                .Entity<Subscribe>()
                .HasIndex(f => new { f.AuthorId, f.FollowerId })
                .IsUnique();
            modelBuilder
                .Entity<LikeToPost>()
                .HasIndex(f => new { f.UserId, f.PostId })
                .IsUnique();
            modelBuilder
                .Entity<LikeToComment>()
                .HasIndex(f => new { f.UserId, f.CommentId })
                .IsUnique();

            modelBuilder.Entity<Subscribe>().ToTable(nameof(Subscribes), t =>
            {
                t.HasCheckConstraint("CK_Subscribes", "\"AuthorId\" <> \"FollowerId\"");
            });

            modelBuilder.Entity<UserConfig>()
                .HasKey(f => f.UserId);

            modelBuilder.Entity<Subscribe>()
                .HasOne(s => s.Author)
                .WithMany(x => x.Followers)
                .HasForeignKey(s => s.AuthorId);
            modelBuilder.Entity<Subscribe>()
                .HasOne(s => s.Follower)
                .WithMany(x => x.Subscribes)
                .HasForeignKey(s => s.FollowerId);
            modelBuilder.Entity<BlackListItem>()
                .HasOne(s => s.User)
                .WithMany(x => x.BlockedUsers)
                .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<BlackListItem>()
                .HasOne(s => s.BlockedUser)
                .WithMany(x => x.BlockedByUsers)
                .HasForeignKey(s => s.BlockedUserId);
            modelBuilder.Entity<MuteListItem>()
                .HasOne(s => s.User)
                .WithMany(x => x.MutedUsers)
                .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<MuteListItem>()
                .HasOne(s => s.MutedUser)
                .WithMany(x => x.MutedByUsers)
                .HasForeignKey(s => s.MutedUserId);

            modelBuilder.Entity<Attach>().UseTptMappingStrategy();
            modelBuilder.Entity<Like>().UseTptMappingStrategy();

            modelBuilder
               .Entity<UserActivity>()
               .ToView("UsersActivity")
               .HasKey(t => t.Id);
            modelBuilder
               .Entity<PostStats>()
               .ToView("PostsStats")
               .HasKey(t => t.Id);
            modelBuilder
               .Entity<CommentStats>()
               .ToView("CommentsStats")
               .HasKey(t => t.Id);
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
        public DbSet<Like> Likes => Set<Like>(); 
        public DbSet<LikeToPost> LikesToPosts => Set<LikeToPost>();
        public DbSet<LikeToComment> LikesToComments => Set<LikeToComment>();
        public DbSet<Subscribe> Subscribes => Set<Subscribe>();
        public DbSet<BlackListItem> BlackList => Set<BlackListItem>();
        public DbSet<MuteListItem> MuteList => Set<MuteListItem>();
        public DbSet<UserConfig> UsersConfigs => Set<UserConfig>(); 
        public DbSet<UserActivity> UsersActivity => Set<UserActivity>();
        public DbSet<PostStats> PostsStats => Set<PostStats>();
        public DbSet<CommentStats> CommentsStats => Set<CommentStats>();
    }
}