using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrivacySettingsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "search_engine_indexing",
                table: "user_privacy_settings",
                newName: "search_indexing");

            migrationBuilder.RenameColumn(
                name: "location_sharing",
                table: "user_privacy_settings",
                newName: "location_visibility");

            migrationBuilder.RenameColumn(
                name: "contact_information",
                table: "user_privacy_settings",
                newName: "contact_visibility");

            migrationBuilder.RenameColumn(
                name: "activity_streaks",
                table: "user_privacy_settings",
                newName: "activity_streaks_visibility");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "search_indexing",
                table: "user_privacy_settings",
                newName: "search_engine_indexing");

            migrationBuilder.RenameColumn(
                name: "location_visibility",
                table: "user_privacy_settings",
                newName: "location_sharing");

            migrationBuilder.RenameColumn(
                name: "contact_visibility",
                table: "user_privacy_settings",
                newName: "contact_information");

            migrationBuilder.RenameColumn(
                name: "activity_streaks_visibility",
                table: "user_privacy_settings",
                newName: "activity_streaks");
        }
    }
}
