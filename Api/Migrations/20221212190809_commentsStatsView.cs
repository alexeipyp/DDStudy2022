using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class commentsStatsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.\"CommentsStats\" AS SELECT c.\"Id\", c.\"PostId\", " +
            "COALESCE(l.\"LA\", 0) AS \"LikesAmount\" " +
            "FROM public.\"Comments\" c " +
            "LEFT JOIN(select \"CommentId\", count(\"CommentId\") AS \"LA\"  from public.\"LikesToComments\" group by \"CommentId\") l " +
            "ON c.\"Id\" = l.\"CommentId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"CommentsStats\";");
        }
    }
}
