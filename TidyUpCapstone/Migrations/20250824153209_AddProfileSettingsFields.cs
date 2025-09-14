using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileSettingsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "birthday",
                table: "app_user",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "app_user",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "app_user",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "app_user",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "app_user",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "app_user",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_code",
                table: "app_user",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "verification_code_expiry",
                table: "app_user",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birthday",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "location",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "verification_code",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "verification_code_expiry",
                table: "app_user");
        }
    }
}
