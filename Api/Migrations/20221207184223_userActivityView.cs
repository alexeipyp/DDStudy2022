using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class userActivityView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.\"UsersActivity\" AS SELECT u.\"Id\", " +
                "count(p.\"Id\") AS \"PostsAmount\", count(f.\"AuthorId\") AS \"FollowersAmount\", count(s.\"FollowerId\") AS \"FollowingAmount\" " + 
                "FROM public.\"Users\" u " +
                "LEFT JOIN public.\"Posts\" p ON u.\"Id\" = p.\"AuthorId\" " +
                "LEFT JOIN public.\"Subscribes\" f ON u.\"Id\" = f.\"AuthorId\" " +
                "LEFT JOIN public.\"Subscribes\" s ON u.\"Id\" = s.\"FollowerId\" " +
                "GROUP BY u.\"Id\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"UserActivity\";");
        }
    }
}
