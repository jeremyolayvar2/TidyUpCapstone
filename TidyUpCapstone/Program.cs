using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Services.Background;
using TidyUpCapstone.Services.Helpers;
using TidyUpCapstone.Services.Implementations;
using TidyUpCapstone.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Data Source=Raven-PC\\SQLEXPRESS;Initial Catalog=DBcapstone;Integrated Security=True;Trust Server Certificate=True"
    ));

// Identity Configuration
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false; // Set to false for development
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Session configuration for user testing
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container
builder.Services.AddControllersWithViews();

// Register Gamification Services
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IQuestHelperService, QuestHelperService>();
builder.Services.AddScoped<IUserInitializationService, UserInitializationService>();
builder.Services.AddScoped<IActivityQuestIntegrationService, ActivityQuestIntegrationService>();
builder.Services.AddScoped<IUserStatisticsService, UserStatisticsService>();

// Register Background Service for Quest Management
//builder.Services.AddHostedService<QuestBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware (must be before UseAuthentication)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize gamification system
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
        Console.WriteLine("✅ Levels seeded successfully (100 levels)");
    }
    else
    {
        Console.WriteLine("ℹ️  Levels already seeded");
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

app.Run();