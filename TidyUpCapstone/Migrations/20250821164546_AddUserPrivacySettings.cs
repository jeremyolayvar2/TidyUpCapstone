using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPrivacySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_privacy_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    profile_visibility = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    location_sharing = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    activity_streaks = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    online_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    search_engine_indexing = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    contact_information = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    activity_history = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_privacy_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_privacy_settings_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_privacy_settings_user_id",
                table: "user_privacy_settings",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_privacy_settings");
        }
    }
}
