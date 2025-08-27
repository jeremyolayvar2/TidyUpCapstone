using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageAccessibilitySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "high_contrast",
                table: "app_user",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "app_user",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "large_text",
                table: "app_user",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "reduce_motion",
                table: "app_user",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "screen_reader",
                table: "app_user",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "timezone",
                table: "app_user",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "high_contrast",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "language",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "large_text",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "reduce_motion",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "screen_reader",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "timezone",
                table: "app_user");
        }
    }
}
