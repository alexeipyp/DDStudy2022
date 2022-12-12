using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class postsStatsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.\"PostsStats\" AS SELECT p.\"Id\", " +
                "count(c.\"Id\") AS \"CommentsAmount\", count(l.\"Id\") AS \"LikesAmount\" " +
                "FROM public.\"Posts\" p " +
                "LEFT JOIN public.\"Comments\" c ON p.\"Id\" = c.\"PostId\" " +
                "LEFT JOIN public.\"LikesToPosts\" l ON p.\"Id\" = l.\"PostId\" " +
                "GROUP BY p.\"Id\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"PostsStats\";");
        }
    }
}
