using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Extensions;
using TidyUpCapstone.Helpers;
using TidyUpCapstone.Models.Configuration;
using TidyUpCapstone.Models.DTOs.Configuration;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Services;
using TidyUpCapstone.Services.Background;
using TidyUpCapstone.Services.Helpers;
using TidyUpCapstone.Services.Implementations;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Hubs; // Added from chat-page branch

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------
// 1. Enhanced Logging Configuration
// -----------------------------------------------------
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();

    if (builder.Environment.IsDevelopment())
    {
        logging.SetMinimumLevel(LogLevel.Debug);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    }
    else
    {
        logging.SetMinimumLevel(LogLevel.Information);
    }
});

// -----------------------------------------------------
// 2. Configure Core Services using Extension Methods (from dev)
// -----------------------------------------------------
builder.Services
    .AddDatabaseServices(builder.Configuration)
    .AddIdentityServices()  // This configures Identity properly
    .AddTidyUpServices()
    .AddFileUploadConfiguration()
    .AddApiConfiguration()
    .AddLoggingConfiguration(builder.Environment)
    .AddCachingServices()
    .AddAIServices(builder.Configuration)
    .AddBackgroundServices();

// -----------------------------------------------------
// 3. Register Item Management Services (from dev)
// -----------------------------------------------------
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IViewModelService, ViewModelService>();
builder.Services.AddScoped<IVisionService, VisionService>();
builder.Services.AddScoped<IVertexAiService, VertexAiService>();

// Register ILanguageService if it exists
try
{
    builder.Services.AddScoped<ILanguageService, LanguageService>();
}
catch
{
    // Service might not exist yet - ignore for now
}

// -----------------------------------------------------
// 4. Register Gamification Services (from dev branch)
// -----------------------------------------------------
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IQuestHelperService, QuestHelperService>();
builder.Services.AddScoped<IUserInitializationService, UserInitializationService>();
builder.Services.AddScoped<IActivityQuestIntegrationService, ActivityQuestIntegrationService>();
builder.Services.AddScoped<IUserStatisticsService, UserStatisticsService>();

// -----------------------------------------------------
// 5. MERGED: Add Leaderboard Services (from feature/leaderboards branch)
// -----------------------------------------------------
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

// -----------------------------------------------------
// 6. MERGED: Transaction and Chat Services (from chat-page branch)
// -----------------------------------------------------
builder.Services.AddScoped<IEscrowService, EscrowService>();

// -----------------------------------------------------
// 7. MERGED: Add SignalR (from chat-page branch)
// -----------------------------------------------------
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

// -----------------------------------------------------
// 8. Application Cookie Configuration FOR MODAL SYSTEM (from dev)
// -----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/";

    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Authentication required",
                requiresLogin = true
            });
            return context.Response.WriteAsync(jsonResponse);
        }

        var returnUrl = context.RedirectUri;
        if (!string.IsNullOrEmpty(returnUrl))
        {
            context.Response.Redirect($"/?showLogin=true&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        else
        {
            context.Response.Redirect("/?showLogin=true");
        }
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Access denied",
                accessDenied = true
            });
            return context.Response.WriteAsync(jsonResponse);
        }

        context.Response.Redirect("/?error=access_denied");
        return Task.CompletedTask;
    };
});

// -----------------------------------------------------
// 9. Enhanced External Authentication Configuration (from dev)
// -----------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ExternalScheme;
})
.AddGoogle(googleOptions =>
{
    var clientId = builder.Configuration["Authentication:Google:ClientId"];
    var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
    {
        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var startupLogger = loggerFactory.CreateLogger("Program");
        startupLogger.LogWarning("Google OAuth credentials are not properly configured. Google authentication will not work.");
    }
    else
    {
        googleOptions.ClientId = clientId;
        googleOptions.ClientSecret = clientSecret;
        googleOptions.SaveTokens = true;
        googleOptions.Scope.Add("email");
        googleOptions.Scope.Add("profile");

        googleOptions.Events.OnCreatingTicket = context =>
        {
            var contextLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            contextLogger.LogInformation("Google OAuth ticket created for user: {Email}",
                context.Principal?.FindFirstValue(ClaimTypes.Email) ?? "unknown");
            return Task.CompletedTask;
        };

        googleOptions.Events.OnRemoteFailure = context =>
        {
            var contextLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            contextLogger.LogError("Google OAuth remote failure: {Error}", context.Failure?.Message ?? "Unknown error");

            var errorMessage = "oauth_failed";
            if (context.Failure?.Message?.Contains("access_denied") == true)
            {
                errorMessage = "access_denied";
            }

            context.Response.Redirect($"/?error={errorMessage}&showLogin=true");
            context.HandleResponse();
            return Task.CompletedTask;
        };

        googleOptions.Events.OnTicketReceived = context =>
        {
            var contextLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            contextLogger.LogInformation("Google OAuth ticket received successfully");
            return Task.CompletedTask;
        };

        googleOptions.Events.OnAccessDenied = context =>
        {
            var contextLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            contextLogger.LogWarning("Google OAuth access denied");

            context.Response.Redirect("/?error=access_denied&showLogin=true");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    }
});

// -----------------------------------------------------
// 10. Email Service Configuration (SendGrid) (from dev)
// -----------------------------------------------------
builder.Services.Configure<SendGridSettingsDto>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddSingleton<ISendGridClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<SendGridSettingsDto>>().Value;
    var serviceLogger = provider.GetRequiredService<ILogger<Program>>();

    if (string.IsNullOrEmpty(settings.ApiKey))
    {
        serviceLogger.LogWarning("SendGrid API key is not configured. Email functionality will not work.");
    }
    else
    {
        serviceLogger.LogInformation("SendGrid client configured successfully");
    }

    return new SendGridClient(settings.ApiKey ?? "");
});

builder.Services.AddScoped<IEmailService, EmailService>();

// -----------------------------------------------------
// 11. Google Cloud and Vision Services Configuration (from dev)
// -----------------------------------------------------
builder.Services.Configure<GoogleCloudSetting>(
    builder.Configuration.GetSection("GoogleCloud"));
builder.Services.Configure<VisionSettings>(
    builder.Configuration.GetSection("VisionSettings"));

// Set Google Cloud credentials environment variable
var googleCloudSettings = builder.Configuration.GetSection("GoogleCloud").Get<GoogleCloudSetting>();
if (!string.IsNullOrEmpty(googleCloudSettings?.CredentialsPath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleCloudSettings.CredentialsPath);
}

// -----------------------------------------------------
// 12. MERGED: Session Support and CORS (combined from both)
// -----------------------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add CORS for SignalR (from chat-page branch)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy
            .WithOrigins("https://localhost:5001", "http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// -----------------------------------------------------
// 13. MVC Configuration (combined)
// -----------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    if (!builder.Environment.IsDevelopment())
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequireHttpsAttribute());
    }
});

builder.Services.AddControllers(); // For API controllers

// Add antiforgery token configuration (merged from both approaches)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken"; // From chat-page branch
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// -----------------------------------------------------
// 14. Build Application
// -----------------------------------------------------
var app = builder.Build();

// Get logger for this scope
var appLogger = app.Services.GetRequiredService<ILogger<Program>>();

// -----------------------------------------------------
// 15. Database Initialization with Enhanced Error Handling (merged approach)
// -----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var scopeLogger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        scopeLogger.LogInformation("Initializing database...");
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Check if database exists and is accessible
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            scopeLogger.LogError("Cannot connect to database. Check connection string and SQL Server status.");
            if (!app.Environment.IsDevelopment())
                throw new InvalidOperationException("Database connection failed");
        }
        else
        {
            if (app.Environment.IsDevelopment())
            {
                // In development, ensure database is created (no automatic seeding)
                await context.Database.EnsureCreatedAsync();
                scopeLogger.LogInformation("Database created successfully");
            }
            else
            {
                // In production, apply pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    scopeLogger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                }
            }

            scopeLogger.LogInformation("Database initialized successfully");

            // Seed initial data if needed
            await SeedInitialData(services, scopeLogger);
        }
    }
    catch (Exception ex)
    {
        scopeLogger.LogError(ex, "Error during database initialization: {Message}", ex.Message);

        if (app.Environment.IsDevelopment())
        {
            scopeLogger.LogWarning("Continuing in development mode despite database error");
        }
        else
        {
            throw;
        }
    }
}

// -----------------------------------------------------
// 16. Initialize Gamification System (from dev branch)
// -----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var achievementService = scope.ServiceProvider.GetRequiredService<IAchievementService>();
        var streakService = scope.ServiceProvider.GetRequiredService<IStreakService>();
        var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();
        var userInitService = scope.ServiceProvider.GetRequiredService<IUserInitializationService>();

        appLogger.LogInformation("Starting gamification system initialization...");

        // Seed levels first (required for user level progress)
        await SeedLevelsAsync(context);

        // Seed achievements if not already seeded
        if (!await achievementService.AreAchievementsSeededAsync())
        {
            await achievementService.SeedAchievementsAsync();
            appLogger.LogInformation("Achievements seeded successfully");
        }
        else
        {
            appLogger.LogInformation("Achievements already seeded");
        }

        // Seed streak types if not already seeded
        if (!await streakService.AreStreakTypesSeededAsync())
        {
            await streakService.SeedStreakTypesAsync();
            appLogger.LogInformation("Streak types seeded successfully");
        }
        else
        {
            appLogger.LogInformation("Streak types already seeded");
        }

        // Generate initial quests
        await questService.GenerateDailyQuestsAsync();
        appLogger.LogInformation("Daily quests generated");

        await questService.GenerateWeeklyQuestsAsync();
        appLogger.LogInformation("Weekly quests generated");

        // Auto-initialize all existing users with achievements and stats
        var users = await context.Users.ToListAsync();
        int initializedCount = 0;

        foreach (var user in users)
        {
            var wasInitialized = !await userInitService.IsUserInitializedAsync(user.Id);
            await userInitService.InitializeUserAsync(user.Id);
            if (wasInitialized)
            {
                initializedCount++;
            }
        }

        if (initializedCount > 0)
        {
            appLogger.LogInformation("Auto-initialized {Count} users with achievements and stats", initializedCount);
        }
        else
        {
            appLogger.LogInformation("All {Count} users already initialized", users.Count);
        }

        appLogger.LogInformation("Gamification system initialized successfully!");
        appLogger.LogInformation("System Status:");
        appLogger.LogInformation("   - Users: {UserCount}", users.Count);
        appLogger.LogInformation("   - Achievements: {AchievementCount}", await context.Achievements.CountAsync());
        appLogger.LogInformation("   - Active Quests: {QuestCount}", await context.Quests.CountAsync(q => q.IsActive));
        appLogger.LogInformation("   - Levels: {LevelCount}", await context.Levels.CountAsync());
    }
    catch (Exception ex)
    {
        appLogger.LogError(ex, "Error during gamification system initialization: {Message}", ex.Message);

        // Don't stop the application, just log the error
        appLogger.LogWarning("Failed to initialize gamification system");
    }
}

// -----------------------------------------------------
// 17. Configure HTTP Request Pipeline (merged)
// -----------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// MERGED: Add CORS for SignalR (from chat-page branch)
app.UseCors("SignalRCorsPolicy");

// Session middleware (must be after UseRouting and before UseAuthentication)
app.UseSession();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------
// 18. MERGED: Configure Routes and SignalR Hub (combined)
// -----------------------------------------------------
app.MapControllers(); // For API controllers

// Map SignalR Hub (from chat-page branch)
app.MapHub<ChatHub>("/chathub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Index}",
    defaults: new { controller = "Account", action = "Index" });

app.MapControllerRoute(
    name: "main",
    pattern: "Main",
    defaults: new { controller = "Home", action = "Main" });

app.MapControllerRoute(
    name: "login",
    pattern: "Login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "Register",
    defaults: new { controller = "Account", action = "Register" });

// Health check endpoint
app.MapGet("/health", async (ApplicationDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            database = "connected"
        }) : Results.Problem("Database connection failed");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Health check failed: {ex.Message}");
    }
});

// Initialize database in development using extension method (from dev)
if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

// -----------------------------------------------------
// 19. Helper Methods (from dev branch)
// -----------------------------------------------------

// Helper method for seeding levels
static async Task SeedLevelsAsync(ApplicationDbContext context)
{
    if (!await context.Levels.AnyAsync())
    {
        var levels = new List<Level>();

        for (int i = 1; i <= 100; i++)
        {
            var level = new Level
            {
                LevelNumber = i,
                LevelName = GetLevelName(i),
                XpRequired = CalculateXpRequired(i),
                XpToNext = CalculateXpToNext(i),
                TokenBonus = CalculateTokenBonus(i)
            };
            levels.Add(level);
        }

        context.Levels.AddRange(levels);
        await context.SaveChangesAsync();
        Console.WriteLine("Levels seeded successfully (100 levels)");
    }
    else
    {
        Console.WriteLine("Levels already seeded");
    }
}

static async Task SeedInitialData(IServiceProvider services, ILogger logger)
{
    try
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Create default roles if they don't exist
        var roles = new[] { "Admin", "User", "Moderator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }

        logger.LogInformation("Initial data seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during initial data seeding");
    }
}

static string GetLevelName(int levelNumber)
{
    return levelNumber switch
    {
        1 => "Rookie Organizer",
        2 => "Tidy Apprentice",
        3 => "Clutter Buster",
        4 => "Space Cadet",
        5 => "Order Keeper",
        10 => "Organization Novice",
        15 => "Tidiness Expert",
        20 => "Space Master",
        25 => "Declutter Champion",
        30 => "Organization Guru",
        50 => "Tidiness Legend",
        75 => "Organization Virtuoso",
        100 => "Marie Kondo Incarnate",
        _ => $"Organization Level {levelNumber}"
    };
}

static int CalculateXpRequired(int level)
{
    if (level == 1) return 0;

    int totalXp = 0;
    for (int i = 2; i <= level; i++)
    {
        totalXp += CalculateXpToNext(i - 1);
    }
    return totalXp;
}

static int CalculateXpToNext(int currentLevel)
{
    // Progressive XP scaling for balanced gameplay
    if (currentLevel <= 10)
        return 100 + (currentLevel * 10); // 110, 120, 130... 200
    else if (currentLevel <= 25)
        return 200 + ((currentLevel - 10) * 20); // 220, 240, 260... 500
    else if (currentLevel <= 50)
        return 500 + ((currentLevel - 25) * 25); // 525, 550, 575... 1125
    else if (currentLevel <= 75)
        return 1125 + ((currentLevel - 50) * 35); // 1160, 1195... 2000
    else if (currentLevel <= 90)
        return 2000 + ((currentLevel - 75) * 50); // 2050, 2100... 2750
    else if (currentLevel <= 99)
        return 2750 + ((currentLevel - 90) * 75); // 2825, 2900... 3425
    else
        return 5000; // Max level
}

static decimal CalculateTokenBonus(int level)
{
    // Token bonus increases every 10 levels
    return (decimal)(level / 10) * 0.05m; // 0%, 5%, 10%, 15%, etc.
}

// -----------------------------------------------------
// 20. Start Application
// -----------------------------------------------------
appLogger.LogInformation("TidyUp application starting...");
appLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
appLogger.LogInformation("Authentication configured with OAuth and modal system");
appLogger.LogInformation("Item management, gamification, leaderboards, and chat features enabled");

app.Run();