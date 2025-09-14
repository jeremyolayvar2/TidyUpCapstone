using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_settings",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    email_new_messages = table.Column<bool>(type: "bit", nullable: false),
                    email_item_updates = table.Column<bool>(type: "bit", nullable: false),
                    email_weekly_summary = table.Column<bool>(type: "bit", nullable: false),
                    desktop_notifications = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_notification_settings_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_notification_settings_user",
                table: "notification_settings",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_settings");
        }
    }
}
