using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "AnalysisResult",
                table: "azure_cv_analysis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "azure_cv_analysis",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "azure_cv_analysis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "AnalysisResult",
                table: "azure_cv_analysis");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "azure_cv_analysis");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "azure_cv_analysis");
        }
    }
}
