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
                "count(l.\"Id\") AS \"LikesAmount\" " +
                "FROM public.\"Comments\" c " +
                "LEFT JOIN public.\"LikesToComments\" l ON c.\"Id\" = l.\"CommentId\" " +
                "GROUP BY c.\"Id\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.\"CommentsStats\";");
        }
    }
}
