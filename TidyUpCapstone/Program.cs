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
// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Data Source=Raven-PC\\SQLEXPRESS;Initial Catalog=DBcapstone;Integrated Security=True;Trust Server Certificate=True"
    ));

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
// 4. Register Gamification Services (from quest page)
// -----------------------------------------------------
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IQuestHelperService, QuestHelperService>();
builder.Services.AddScoped<IUserInitializationService, UserInitializationService>();
builder.Services.AddScoped<IActivityQuestIntegrationService, ActivityQuestIntegrationService>();
builder.Services.AddScoped<IUserStatisticsService, UserStatisticsService>();

// Register Background Service for Quest Management (commented out for now)
//builder.Services.AddHostedService<QuestBackgroundService>();

// -----------------------------------------------------
// 5. Application Cookie Configuration FOR MODAL SYSTEM (from dev)
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
// 6. Enhanced External Authentication Configuration (from dev)
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
// 7. Email Service Configuration (SendGrid) (from dev)
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
// 8. Google Cloud and Vision Services Configuration (from dev)
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
// 9. Session Support (combined from both)
// -----------------------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// -----------------------------------------------------
// 10. MVC Configuration (combined)
// -----------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    if (!builder.Environment.IsDevelopment())
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequireHttpsAttribute());
    }
});

builder.Services.AddControllers(); // For API controllers

// Add antiforgery token configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
// Register Background Service for Quest Management
builder.Services.AddHostedService<QuestBackgroundService>();
builder.Services.AddHostedService<UserSpecialQuestService>();

// -----------------------------------------------------
// 11. Build Application
// -----------------------------------------------------
var app = builder.Build();

// Get logger for this scope
var appLogger = app.Services.GetRequiredService<ILogger<Program>>();

// -----------------------------------------------------
// 12. Database Initialization with Enhanced Error Handling (merged approach)
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
                // In development, ensure database is created and seed data
                await context.Database.EnsureCreatedAsync();
                await DatabaseSeeder.SeedAsync(context);
                scopeLogger.LogInformation("Database seeding completed successfully");
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
// 13. Initialize Gamification System (from quest page)
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

        Console.WriteLine("🚀 Starting gamification system initialization...");

        // Seed levels first (required for user level progress)
        await SeedLevelsAsync(context);

        // Seed achievements if not already seeded
        if (!await achievementService.AreAchievementsSeededAsync())
        {
            await achievementService.SeedAchievementsAsync();
            Console.WriteLine("✅ Achievements seeded successfully");
        }
        else
        {
            Console.WriteLine("ℹ️  Achievements already seeded");
        }

        // Seed streak types if not already seeded
        if (!await streakService.AreStreakTypesSeededAsync())
        {
            await streakService.SeedStreakTypesAsync();
            Console.WriteLine("✅ Streak types seeded successfully");
        }
        else
        {
            Console.WriteLine("ℹ️  Streak types already seeded");
        }

        // Generate initial quests
        await questService.GenerateDailyQuestsAsync();
        Console.WriteLine("✅ Daily quests generated");

        await questService.GenerateWeeklyQuestsAsync();
        Console.WriteLine("✅ Weekly quests generated");

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
            Console.WriteLine($"✅ Auto-initialized {initializedCount} users with achievements and stats");
        }
        else
        {
            Console.WriteLine($"ℹ️  All {users.Count} users already initialized");
        }

        Console.WriteLine("🎉 Gamification system initialized successfully!");
        Console.WriteLine("📊 System Status:");
        Console.WriteLine($"   - Users: {users.Count}");
        Console.WriteLine($"   - Achievements: {await context.Achievements.CountAsync()}");
        Console.WriteLine($"   - Active Quests: {await context.Quests.CountAsync(q => q.IsActive)}");
        Console.WriteLine($"   - Levels: {await context.Levels.CountAsync()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during gamification system initialization: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");

        // Don't stop the application, just log the error
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Failed to initialize gamification system");
    }
}

// -----------------------------------------------------
// 14. Configure HTTP Request Pipeline (merged)
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

// Session middleware (must be after UseRouting and before UseAuthentication)
app.UseSession();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------
// 15. Configure Routes (merged)
// -----------------------------------------------------
app.MapControllers(); // For API controllers

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
// 16. Helper Methods
// -----------------------------------------------------

// Helper method for seeding levels (from quest page)
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
        Console.WriteLine("✅ Levels seeded successfully (100 levels)");
    }
    else
    {
        Console.WriteLine("ℹ️  Levels already seeded");
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
        6 => "Neat Enthusiast",
        7 => "Sorting Specialist",
        8 => "Organization Adept",
        9 => "Declutter Detective",
        10 => "Tidiness Scholar",
        11 => "Marie's Student",
        12 => "Joy Seeker",
        13 => "Gratitude Practitioner",
        14 => "Folding Artist",
        15 => "Category Master",
        16 => "Clothing Curator",
        17 => "Book Librarian",
        18 => "Paper Archivist",
        19 => "Kimono Keeper",
        20 => "Joy Spark Detective",
        21 => "Visible Storage Expert",
        22 => "Box Organization Pro",
        23 => "Size Sorting Sage",
        24 => "Gratitude Guru",
        25 => "KonMari Devotee",
        26 => "Transformation Tracker",
        27 => "Mindful Organizer",
        28 => "Peaceful Space Creator",
        29 => "Harmony Architect",
        30 => "Joy Ambassador",
        31 => "Fold Perfectionist",
        32 => "Category Conqueror",
        33 => "Closet Commander",
        34 => "Bookshelf Builder",
        35 => "Document Director",
        36 => "Kitchen Coordinator",
        37 => "Bathroom Beautifier",
        38 => "Garage Guardian",
        39 => "Miscellaneous Manager",
        40 => "Home Harmonizer",
        41 => "Space Sanctuary Creator",
        42 => "Joy Journey Guide",
        43 => "Minimalist Mentor",
        44 => "Organization Oracle",
        45 => "Tidiness Teacher",
        46 => "Declutter Disciple",
        47 => "Marie's Messenger",
        48 => "Life Lifestyle Designer",
        49 => "Spark Joy Specialist",
        50 => "KonMari Knight",
        51 => "Zen Zone Creator",
        52 => "Peaceful Place Planner",
        53 => "Serenity Specialist",
        54 => "Calm Curator",
        55 => "Tranquil Transformer",
        56 => "Mindful Master",
        57 => "Gratitude Guardian",
        58 => "Joy Journey Master",
        59 => "Blissful Builder",
        60 => "Harmony Hero",
        61 => "Organization Overlord",
        62 => "Tidiness Titan",
        63 => "Declutter Deity",
        64 => "Space Saint",
        65 => "Order Oracle",
        66 => "Category Champion",
        67 => "Joy Jedi",
        68 => "KonMari Sage",
        69 => "Transformation Titan",
        70 => "Life-Changing Legend",
        71 => "Spark Joy Sovereign",
        72 => "Mindfulness Monarch",
        73 => "Gratitude God",
        74 => "Organization Olympian",
        75 => "Tidiness Transcendent",
        76 => "Declutter Divinity",
        77 => "Space Shaman",
        78 => "Joy Journey Genius",
        79 => "KonMari Keeper",
        80 => "Zen Master Supreme",
        81 => "Harmony Hierophant",
        82 => "Peace Prophet",
        83 => "Serenity Sovereign",
        84 => "Tranquil Transcendent",
        85 => "Calm Cosmic Force",
        86 => "Mindful Universe",
        87 => "Gratitude Galaxy",
        88 => "Joy Journey Cosmos",
        89 => "Organization Omega",
        90 => "Tidiness Infinity",
        91 => "Declutter Dimension",
        92 => "Space Singularity",
        93 => "KonMari Consciousness",
        94 => "Life-Changing Luminary",
        95 => "Spark Joy Celestial",
        96 => "Transformation Transcendent",
        97 => "Harmony Hypernova",
        98 => "Zen Zenith",
        99 => "Ultimate Joy Master",
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
// 17. Start Application
// -----------------------------------------------------
appLogger.LogInformation("TidyUp application starting...");
appLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
appLogger.LogInformation("Authentication configured with OAuth and modal system");
appLogger.LogInformation("Item management and gamification features enabled");

app.Run();