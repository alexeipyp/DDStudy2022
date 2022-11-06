using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Console.WriteLine($"<< Data Context: {Guid.NewGuid()} >>");
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


            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("Api"));

        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
    }
}