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
            "COALESCE(p.\"PostsAmount\", 0) AS \"PostsAmount\", " +
            "COALESCE(f.\"FollowersAmount\", 0) AS \"FollowersAmount\", " +
            "COALESCE(s.\"FollowingAmount\", 0) AS \"FollowingAmount\" " +
            "FROM public.\"Users\" u " +
            "LEFT JOIN(select \"AuthorId\", count(\"AuthorId\") AS \"PostsAmount\" from public.\"Posts\" group by \"AuthorId\") p " +
            "ON u.\"Id\" = p.\"AuthorId\" " +
            "LEFT JOIN(select \"AuthorId\", count(\"AuthorId\") AS \"FollowersAmount\" from public.\"Subscribes\" where \"IsAccepted\" = true group by \"AuthorId\") f " +
            "ON u.\"Id\" = f.\"AuthorId\" " +
            "LEFT JOIN(select \"FollowerId\", count(\"FollowerId\") AS \"FollowingAmount\" from public.\"Subscribes\" where \"IsAccepted\" = true group by \"FollowerId\") s " +
            "ON u.\"Id\" = s.\"FollowerId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"UsersActivity\";");
        }
    }
}
