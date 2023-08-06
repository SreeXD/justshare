using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustShareBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Posts",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "ImageId",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Posts",
                newName: "Title");
        }
    }
}
