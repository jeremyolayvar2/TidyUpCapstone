using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "posts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "posts");
        }
    }
}
