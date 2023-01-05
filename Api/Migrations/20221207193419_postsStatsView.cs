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
            "COALESCE(c.\"CA\", 0) AS \"CommentsAmount\", COALESCE(l.\"LA\", 0) AS \"LikesAmount\" " +
            "FROM public.\"Posts\" p " +
            "LEFT JOIN(select \"PostId\", count(\"PostId\") AS \"CA\" from public.\"Comments\" group by \"PostId\") c " +
            "ON p.\"Id\" = c.\"PostId\" " +
            "LEFT JOIN(select \"PostId\", count(\"PostId\") AS \"LA\" from public.\"LikesToPosts\" group by \"PostId\") l " +
            "ON p.\"Id\" = l.\"PostId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"PostsStats\";");
        }
    }
}
