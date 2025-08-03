using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class FixIdentityTableNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_app_user_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_app_user_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_app_user_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_app_user_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "app_user_tokens");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "app_user_roles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "app_user_logins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "app_user_claims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "app_roles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "app_role_claims");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "app_user_roles",
                newName: "IX_app_user_roles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "app_user_logins",
                newName: "IX_app_user_logins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "app_user_claims",
                newName: "IX_app_user_claims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "app_role_claims",
                newName: "IX_app_role_claims_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_app_user_tokens",
                table: "app_user_tokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_app_user_roles",
                table: "app_user_roles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_app_user_logins",
                table: "app_user_logins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_app_user_claims",
                table: "app_user_claims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_app_roles",
                table: "app_roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_app_role_claims",
                table: "app_role_claims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_app_role_claims_app_roles_RoleId",
                table: "app_role_claims",
                column: "RoleId",
                principalTable: "app_roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_claims_app_user_UserId",
                table: "app_user_claims",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_logins_app_user_UserId",
                table: "app_user_logins",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_roles_app_roles_RoleId",
                table: "app_user_roles",
                column: "RoleId",
                principalTable: "app_roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_roles_app_user_UserId",
                table: "app_user_roles",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_tokens_app_user_UserId",
                table: "app_user_tokens",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_role_claims_app_roles_RoleId",
                table: "app_role_claims");

            migrationBuilder.DropForeignKey(
                name: "FK_app_user_claims_app_user_UserId",
                table: "app_user_claims");

            migrationBuilder.DropForeignKey(
                name: "FK_app_user_logins_app_user_UserId",
                table: "app_user_logins");

            migrationBuilder.DropForeignKey(
                name: "FK_app_user_roles_app_roles_RoleId",
                table: "app_user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_app_user_roles_app_user_UserId",
                table: "app_user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_app_user_tokens_app_user_UserId",
                table: "app_user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app_user_tokens",
                table: "app_user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app_user_roles",
                table: "app_user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app_user_logins",
                table: "app_user_logins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app_user_claims",
                table: "app_user_claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app_roles",
                table: "app_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app_role_claims",
                table: "app_role_claims");

            migrationBuilder.RenameTable(
                name: "app_user_tokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "app_user_roles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "app_user_logins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "app_user_claims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "app_roles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "app_role_claims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameIndex(
                name: "IX_app_user_roles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_app_user_logins_UserId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_app_user_claims_UserId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_app_role_claims_RoleId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_app_user_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_app_user_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_app_user_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_app_user_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
