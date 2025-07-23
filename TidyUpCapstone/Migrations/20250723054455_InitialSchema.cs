using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TidyUpCapstone.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CriteriaType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CriteriaValue = table.Column<int>(type: "int", nullable: false),
                    TokenReward = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    XpReward = table.Column<int>(type: "int", nullable: true),
                    BadgeImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSecret = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token_balance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    managed_by_admin_id = table.Column<int>(type: "int", nullable: true),
                    admin_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.user_id);
                    table.CheckConstraint("chk_positive_token_balance", "token_balance >= 0");
                    table.ForeignKey(
                        name: "FK_app_user_app_user_managed_by_admin_id",
                        column: x => x.managed_by_admin_id,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "item_category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "itemcondition",
                columns: table => new
                {
                    condition_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    condition_multiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemcondition", x => x.condition_id);
                });

            migrationBuilder.CreateTable(
                name: "itemlocation",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemlocation", x => x.location_id);
                });

            migrationBuilder.CreateTable(
                name: "leaderboards",
                columns: table => new
                {
                    LeaderboardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Metric = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetFrequency = table.Column<int>(type: "int", nullable: false),
                    LastReset = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReset = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxEntries = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaderboards", x => x.LeaderboardId);
                });

            migrationBuilder.CreateTable(
                name: "levels",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    XpRequired = table.Column<int>(type: "int", nullable: false),
                    XpToNext = table.Column<int>(type: "int", nullable: false),
                    TitleUnlock = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenBonus = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SpecialPrivilege = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_levels", x => x.LevelId);
                });

            migrationBuilder.CreateTable(
                name: "notification_types",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    default_enabled = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_types", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "quests",
                columns: table => new
                {
                    QuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuestType = table.Column<int>(type: "int", nullable: false),
                    QuestDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestObjective = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TokenReward = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    XpReward = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quests", x => x.QuestId);
                });

            migrationBuilder.CreateTable(
                name: "sso_providers",
                columns: table => new
                {
                    ProviderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClientSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AuthorityUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sso_providers", x => x.ProviderId);
                    table.UniqueConstraint("AK_sso_providers_ProviderName", x => x.ProviderName);
                });

            migrationBuilder.CreateTable(
                name: "streak_types",
                columns: table => new
                {
                    StreakTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreakUnit = table.Column<int>(type: "int", nullable: false),
                    BaseRewards = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MilestoneRewards = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MilestoneInterval = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streak_types", x => x.StreakTypeId);
                });

            migrationBuilder.CreateTable(
                name: "visual_items",
                columns: table => new
                {
                    VisualId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisualName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VisualDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VisualPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VisualImgUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VisualType = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visual_items", x => x.VisualId);
                });

            migrationBuilder.CreateTable(
                name: "admin",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdminRole = table.Column<int>(type: "int", nullable: false),
                    AdminPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAdminLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminStatus = table.Column<int>(type: "int", nullable: false),
                    CanManageSso = table.Column<bool>(type: "bit", nullable: false),
                    LastPasswordChange = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin", x => x.AdminId);
                    table.ForeignKey(
                        name: "FK_admin_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email_verifications",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    Expiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerificationType = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RedirectUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verifications", x => x.VerificationId);
                    table.ForeignKey(
                        name: "FK_email_verifications_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "login_logs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    login_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    session_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    login_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    logout_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    failure_reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    device_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    browser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    os = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_logs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_login_logs_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    PostContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostType = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEdited = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_posts_app_user_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "user_achievements",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    IsNotified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_achievements", x => new { x.UserId, x.AchievementId });
                    table.ForeignKey(
                        name: "FK_user_achievements_achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_achievements_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_location_preferences",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PreferredLat = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    PreferredLng = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    RadiusKm = table.Column<int>(type: "int", nullable: false),
                    AddressDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoDetectLocation = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_location_preferences", x => x.UserId);
                    table.CheckConstraint("chk_valid_radius", "[RadiusKm] > 0 AND [RadiusKm] <= 1000");
                    table.ForeignKey(
                        name: "FK_user_location_preferences_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ItemTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AdjustedTokenPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FinalTokenPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ImageFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AiSuggestedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PriceOverriddenByUser = table.Column<bool>(type: "bit", nullable: false),
                    AiProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    AiProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AiDetectedCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AiConditionScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    AiConfidenceLevel = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.ItemId);
                    table.CheckConstraint("chk_positive_prices", "[AdjustedTokenPrice] >= 0 AND [FinalTokenPrice] >= 0 AND ([AiSuggestedPrice] IS NULL OR [AiSuggestedPrice] >= 0)");
                    table.CheckConstraint("chk_valid_coordinates", "([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)");
                    table.ForeignKey(
                        name: "FK_items_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_items_item_category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "item_category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_items_itemcondition_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "itemcondition",
                        principalColumn: "condition_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_items_itemlocation_LocationId",
                        column: x => x.LocationId,
                        principalTable: "itemlocation",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_history",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SearchQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    MinPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ResultsCount = table.Column<int>(type: "int", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_history", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_search_history_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_search_history_item_category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "item_category",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_search_history_itemlocation_LocationId",
                        column: x => x.LocationId,
                        principalTable: "itemlocation",
                        principalColumn: "location_id");
                });

            migrationBuilder.CreateTable(
                name: "leaderboard_entries",
                columns: table => new
                {
                    EntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LeaderboardId = table.Column<int>(type: "int", nullable: false),
                    RankPosition = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    PreviousRank = table.Column<int>(type: "int", nullable: true),
                    RankChange = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaderboard_entries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_leaderboard_entries_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_leaderboard_entries_leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalTable: "leaderboards",
                        principalColumn: "LeaderboardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_levels",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CurrentLevelId = table.Column<int>(type: "int", nullable: false),
                    CurrentXp = table.Column<int>(type: "int", nullable: false),
                    TotalXp = table.Column<int>(type: "int", nullable: false),
                    XpToNextLevel = table.Column<int>(type: "int", nullable: false),
                    LevelUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalLevelUps = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_levels", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_levels_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_levels_levels_CurrentLevelId",
                        column: x => x.CurrentLevelId,
                        principalTable: "levels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedEntityType = table.Column<int>(type: "int", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_notifications_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_notification_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "notification_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_preferences",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification_preferences", x => new { x.UserId, x.TypeId });
                    table.ForeignKey(
                        name: "FK_user_notification_preferences_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_notification_preferences_notification_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "notification_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userquests",
                columns: table => new
                {
                    UserQuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CurrentProgress = table.Column<int>(type: "int", nullable: false),
                    DateClaimed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userquests", x => x.UserQuestId);
                    table.ForeignKey(
                        name: "FK_userquests_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userquests_quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "quests",
                        principalColumn: "QuestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sso_audit_logs",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sso_audit_logs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_sso_audit_logs_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_sso_audit_logs_sso_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "sso_providers",
                        principalColumn: "ProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sso_links",
                columns: table => new
                {
                    LinkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProviderEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProviderAvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sso_links", x => x.LinkId);
                    table.ForeignKey(
                        name: "FK_user_sso_links_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_sso_links_sso_providers_ProviderName",
                        column: x => x.ProviderName,
                        principalTable: "sso_providers",
                        principalColumn: "ProviderName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_streaks",
                columns: table => new
                {
                    StreakId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StreakTypeId = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LastActivityDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    TotalMilestonesReached = table.Column<int>(type: "int", nullable: false),
                    LastMilestoneDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_streaks", x => x.StreakId);
                    table.ForeignKey(
                        name: "FK_user_streaks_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_streaks_streak_types_StreakTypeId",
                        column: x => x.StreakTypeId,
                        principalTable: "streak_types",
                        principalColumn: "StreakTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_visuals_purchases",
                columns: table => new
                {
                    UserVisualId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VisualId = table.Column<int>(type: "int", nullable: false),
                    DatePurchased = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEquipped = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_visuals_purchases", x => x.UserVisualId);
                    table.ForeignKey(
                        name: "FK_user_visuals_purchases_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_visuals_purchases_visual_items_VisualId",
                        column: x => x.VisualId,
                        principalTable: "visual_items",
                        principalColumn: "VisualId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedByAdminId = table.Column<int>(type: "int", nullable: false),
                    TargetUserId = table.Column<int>(type: "int", nullable: true),
                    ReporterUserId = table.Column<int>(type: "int", nullable: true),
                    ReportName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    ReportDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportStatus = table.Column<int>(type: "int", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileFormat = table.Column<int>(type: "int", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    DownloadCount = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledFrequency = table.Column<int>(type: "int", nullable: true),
                    NextRunDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_admin_reports_admin_GeneratedByAdminId",
                        column: x => x.GeneratedByAdminId,
                        principalTable: "admin",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_admin_reports_app_user_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_admin_reports_app_user_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AdminId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_audit_logs_admin_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin",
                        principalColumn: "AdminId");
                    table.ForeignKey(
                        name: "FK_audit_logs_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettingType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedByAdminId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_system_settings_admin_UpdatedByAdminId",
                        column: x => x.UpdatedByAdminId,
                        principalTable: "admin",
                        principalColumn: "AdminId");
                });

            migrationBuilder.CreateTable(
                name: "user_reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterId = table.Column<int>(type: "int", nullable: false),
                    ReportedUserId = table.Column<int>(type: "int", nullable: false),
                    ReportedEntityType = table.Column<int>(type: "int", nullable: false),
                    ReportedEntityId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvidenceUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSubmitted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportStatus = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionAction = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_reports", x => x.ReportId);
                    table.CheckConstraint("chk_no_self_report", "[ReporterId] != [ReportedUserId]");
                    table.ForeignKey(
                        name: "FK_user_reports_admin_ResolvedByAdminId",
                        column: x => x.ResolvedByAdminId,
                        principalTable: "admin",
                        principalColumn: "AdminId");
                    table.ForeignKey(
                        name: "FK_user_reports_app_user_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_user_reports_app_user_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCommented = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEdited = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_comments_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_comments_comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "comments",
                        principalColumn: "CommentId");
                    table.ForeignKey(
                        name: "FK_comments_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "PostId");
                });

            migrationBuilder.CreateTable(
                name: "reactions",
                columns: table => new
                {
                    ReactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReactionType = table.Column<int>(type: "int", nullable: false),
                    DateReacted = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions", x => x.ReactionId);
                    table.ForeignKey(
                        name: "FK_reactions_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_reactions_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "PostId");
                });

            migrationBuilder.CreateTable(
                name: "ai_training_feedback",
                columns: table => new
                {
                    AiFeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AiPredictedCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserCorrectedCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AiPredictedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    UserSetPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FeedbackType = table.Column<int>(type: "int", nullable: false),
                    ConfidenceRating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_training_feedback", x => x.AiFeedbackId);
                    table.ForeignKey(
                        name: "FK_ai_training_feedback_app_user_UserId",
                        column: x => x.UserId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_ai_training_feedback_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "azure_cv_analysis",
                columns: table => new
                {
                    AnalysisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    DetectedObjects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetectedCategories = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    ApiRequestId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "int", nullable: true),
                    ApiVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_azure_cv_analysis", x => x.AnalysisId);
                    table.ForeignKey(
                        name: "FK_azure_cv_analysis_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "tensorflow_prediction",
                columns: table => new
                {
                    PredictionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PredictedCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConditionScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EstimatedTokenValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DamageDetected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PredictionConfidence = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    IsTrainingData = table.Column<bool>(type: "bit", nullable: false),
                    ProcessingTimeMs = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tensorflow_prediction", x => x.PredictionId);
                    table.ForeignKey(
                        name: "FK_tensorflow_prediction_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TokenAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TransactionStatus = table.Column<int>(type: "int", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.TransactionId);
                    table.CheckConstraint("chk_buyer_not_seller", "[BuyerId] != [SellerId]");
                    table.CheckConstraint("chk_positive_token_amount", "[TokenAmount] > 0");
                    table.ForeignKey(
                        name: "FK_transactions_app_user_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_app_user_SellerId",
                        column: x => x.SellerId,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quest_progress",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuestId = table.Column<int>(type: "int", nullable: false),
                    ProgressValue = table.Column<int>(type: "int", nullable: false),
                    GoalValue = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActionTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quest_progress", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_quest_progress_userquests_UserQuestId",
                        column: x => x.UserQuestId,
                        principalTable: "userquests",
                        principalColumn: "UserQuestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_processing_pipeline",
                columns: table => new
                {
                    ProcessingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AnalysisId = table.Column<int>(type: "int", nullable: true),
                    PredictionId = table.Column<int>(type: "int", nullable: true),
                    AzureCvStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TensorflowStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FinalCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FinalTokenValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ConfidenceLevel = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_processing_pipeline", x => x.ProcessingId);
                    table.ForeignKey(
                        name: "FK_ai_processing_pipeline_azure_cv_analysis_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "azure_cv_analysis",
                        principalColumn: "AnalysisId");
                    table.ForeignKey(
                        name: "FK_ai_processing_pipeline_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ai_processing_pipeline_tensorflow_prediction_PredictionId",
                        column: x => x.PredictionId,
                        principalTable: "tensorflow_prediction",
                        principalColumn: "PredictionId");
                });

            migrationBuilder.CreateTable(
                name: "chats",
                columns: table => new
                {
                    ChatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EscrowReleased = table.Column<bool>(type: "bit", nullable: false),
                    BuyerConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SellerConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    DateClaimed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscrowAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscrowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_chats_transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_chat_messages_app_user_SenderId",
                        column: x => x.SenderId,
                        principalTable: "app_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_chat_messages_chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "chats",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_UserId",
                table: "admin",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_reports_GeneratedByAdminId",
                table: "admin_reports",
                column: "GeneratedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_reports_ReporterUserId",
                table: "admin_reports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_reports_TargetUserId",
                table: "admin_reports",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_processing_pipeline_AnalysisId",
                table: "ai_processing_pipeline",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_processing_pipeline_ItemId",
                table: "ai_processing_pipeline",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_processing_pipeline_PredictionId",
                table: "ai_processing_pipeline",
                column: "PredictionId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_training_feedback_ItemId",
                table: "ai_training_feedback",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_training_feedback_UserId",
                table: "ai_training_feedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "app_user",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "idx_user_email",
                table: "app_user",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_user_username",
                table: "app_user",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_managed_by_admin_id",
                table: "app_user",
                column: "managed_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "app_user",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_AdminId",
                table: "audit_logs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_azure_cv_analysis_ItemId",
                table: "azure_cv_analysis",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_ChatId",
                table: "chat_messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_SenderId",
                table: "chat_messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_chats_TransactionId",
                table: "chats",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comments_ParentCommentId",
                table: "comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_PostId",
                table: "comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_UserId",
                table: "comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_email_verifications_UserId",
                table: "email_verifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_item_category_status",
                table: "items",
                columns: new[] { "CategoryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_item_coordinates",
                table: "items",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "idx_item_user_status",
                table: "items",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_items_ConditionId",
                table: "items",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_items_LocationId",
                table: "items",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_leaderboard_entries_LeaderboardId",
                table: "leaderboard_entries",
                column: "LeaderboardId");

            migrationBuilder.CreateIndex(
                name: "IX_leaderboard_entries_UserId_LeaderboardId",
                table: "leaderboard_entries",
                columns: new[] { "UserId", "LeaderboardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_login_logs_user_id",
                table: "login_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_notification_user_read",
                table: "notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_TypeId",
                table: "notifications",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_AuthorId",
                table: "posts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_quest_progress_UserQuestId",
                table: "quest_progress",
                column: "UserQuestId");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_PostId_UserId",
                table: "reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reactions_UserId",
                table: "reactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_search_history_CategoryId",
                table: "search_history",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_search_history_LocationId",
                table: "search_history",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_search_history_UserId",
                table: "search_history",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sso_audit_logs_ProviderId",
                table: "sso_audit_logs",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_sso_audit_logs_UserId",
                table: "sso_audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_sso_provider_name",
                table: "sso_providers",
                column: "ProviderName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_UpdatedByAdminId",
                table: "system_settings",
                column: "UpdatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_tensorflow_prediction_ItemId",
                table: "tensorflow_prediction",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "idx_transaction_buyer_status",
                table: "transactions",
                columns: new[] { "BuyerId", "TransactionStatus" });

            migrationBuilder.CreateIndex(
                name: "idx_transaction_seller_status",
                table: "transactions",
                columns: new[] { "SellerId", "TransactionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_ItemId",
                table: "transactions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_user_achievements_AchievementId",
                table: "user_achievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_user_levels_CurrentLevelId",
                table: "user_levels",
                column: "CurrentLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_preferences_TypeId",
                table: "user_notification_preferences",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReportedUserId",
                table: "user_reports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReporterId",
                table: "user_reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ResolvedByAdminId",
                table: "user_reports",
                column: "ResolvedByAdminId");

            migrationBuilder.CreateIndex(
                name: "idx_user_sso_links_provider",
                table: "user_sso_links",
                columns: new[] { "ProviderName", "ProviderUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_sso_links_UserId_ProviderName",
                table: "user_sso_links",
                columns: new[] { "UserId", "ProviderName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_streaks_StreakTypeId",
                table: "user_streaks",
                column: "StreakTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_user_streaks_UserId_StreakTypeId",
                table: "user_streaks",
                columns: new[] { "UserId", "StreakTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_visuals_purchases_UserId_VisualId",
                table: "user_visuals_purchases",
                columns: new[] { "UserId", "VisualId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_visuals_purchases_VisualId",
                table: "user_visuals_purchases",
                column: "VisualId");

            migrationBuilder.CreateIndex(
                name: "IX_userquests_QuestId",
                table: "userquests",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_userquests_UserId_QuestId",
                table: "userquests",
                columns: new[] { "UserId", "QuestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_reports");

            migrationBuilder.DropTable(
                name: "ai_processing_pipeline");

            migrationBuilder.DropTable(
                name: "ai_training_feedback");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "email_verifications");

            migrationBuilder.DropTable(
                name: "leaderboard_entries");

            migrationBuilder.DropTable(
                name: "login_logs");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "quest_progress");

            migrationBuilder.DropTable(
                name: "reactions");

            migrationBuilder.DropTable(
                name: "search_history");

            migrationBuilder.DropTable(
                name: "sso_audit_logs");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "user_achievements");

            migrationBuilder.DropTable(
                name: "user_levels");

            migrationBuilder.DropTable(
                name: "user_location_preferences");

            migrationBuilder.DropTable(
                name: "user_notification_preferences");

            migrationBuilder.DropTable(
                name: "user_reports");

            migrationBuilder.DropTable(
                name: "user_sso_links");

            migrationBuilder.DropTable(
                name: "user_streaks");

            migrationBuilder.DropTable(
                name: "user_visuals_purchases");

            migrationBuilder.DropTable(
                name: "azure_cv_analysis");

            migrationBuilder.DropTable(
                name: "tensorflow_prediction");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "chats");

            migrationBuilder.DropTable(
                name: "leaderboards");

            migrationBuilder.DropTable(
                name: "userquests");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "achievements");

            migrationBuilder.DropTable(
                name: "levels");

            migrationBuilder.DropTable(
                name: "notification_types");

            migrationBuilder.DropTable(
                name: "admin");

            migrationBuilder.DropTable(
                name: "sso_providers");

            migrationBuilder.DropTable(
                name: "streak_types");

            migrationBuilder.DropTable(
                name: "visual_items");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "quests");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "item_category");

            migrationBuilder.DropTable(
                name: "itemcondition");

            migrationBuilder.DropTable(
                name: "itemlocation");
        }
    }
}
