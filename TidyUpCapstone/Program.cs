using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Configuration;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Services;
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
// 2. Database Configuration with Better Error Handling
// -----------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=OLAYVAR\\SQLEXPRESS;Database=TidyUpdb;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;MultipleActiveResultSets=true";

    options.UseSqlServer(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// -----------------------------------------------------
// 3. Enhanced Identity Configuration
// -----------------------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

    // Password settings - more secure for production
    options.Password.RequireDigit = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireLowercase = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireUppercase = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireNonAlphanumeric = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequiredLength = builder.Environment.IsDevelopment() ? 6 : 8;
    options.Password.RequiredUniqueChars = builder.Environment.IsDevelopment() ? 1 : 4;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true when email service is fully configured
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// -----------------------------------------------------
// 4. Application Cookie Configuration
// -----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/"; // Redirect to home page instead of Login action
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/"; // Redirect to home page instead of AccessDenied action
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// -----------------------------------------------------
// 5. Enhanced External Authentication Configuration
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
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Program");
        logger.LogWarning("Google OAuth credentials are not properly configured. Google authentication will not work.");
    }
    else
    {
        googleOptions.ClientId = clientId;
        googleOptions.ClientSecret = clientSecret;
        googleOptions.SaveTokens = true; // Enable for better debugging
        googleOptions.Scope.Add("email");
        googleOptions.Scope.Add("profile");

        // Add event handlers for debugging (simplified without logger)
        googleOptions.Events.OnCreatingTicket = context =>
        {
            // OAuth ticket created successfully
            return Task.CompletedTask;
        };

        googleOptions.Events.OnRemoteFailure = context =>
        {
            var errorMessage = "oauth_failed";
            if (context.Failure?.Message?.Contains("access_denied") == true)
            {
                errorMessage = "access_denied";
            }

            context.Response.Redirect($"/Home?error={errorMessage}&showLogin=true");
            context.HandleResponse();
            return Task.CompletedTask;
        };

        googleOptions.Events.OnTicketReceived = context =>
        {
            // OAuth ticket received successfully
            return Task.CompletedTask;
        };
    }
});

/* Facebook Configuration - Uncomment when ready
.AddFacebook(facebookOptions =>
{
    var appId = builder.Configuration["Authentication:Facebook:AppId"];
    var appSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    
    if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appSecret))
    {
        facebookOptions.AppId = appId;
        facebookOptions.AppSecret = appSecret;
        facebookOptions.Scope.Add("email");
        facebookOptions.Fields.Add("email");
        facebookOptions.Fields.Add("name");
        facebookOptions.Fields.Add("first_name");
        facebookOptions.Fields.Add("last_name");
        facebookOptions.Fields.Add("picture");
        
        facebookOptions.Events.OnCreatingTicket = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Facebook OAuth ticket created");
            return Task.CompletedTask;
        };
    }
});
*/

// -----------------------------------------------------
// 6. Email Service Configuration (SendGrid)
// -----------------------------------------------------
// Configure SendGrid and Email settings
builder.Services.Configure<SendGridSettingsDto>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EmailSettings"));

// Register SendGrid client with validation
builder.Services.AddSingleton<ISendGridClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<SendGridSettingsDto>>().Value;
    var logger = provider.GetRequiredService<ILogger<Program>>();

    if (string.IsNullOrEmpty(settings.ApiKey))
    {
        logger.LogWarning("SendGrid API key is not configured. Email functionality will not work.");
    }

    return new SendGridClient(settings.ApiKey);
});

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();

// -----------------------------------------------------
// 7. Add Additional Services
// -----------------------------------------------------
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
// 8. MVC Configuration
// -----------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    if (!builder.Environment.IsDevelopment())
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequireHttpsAttribute());
    }
});

// Add antiforgery token configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// -----------------------------------------------------
// 9. Build Application
// -----------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------
// 10. Database Initialization with Better Error Handling
// -----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing database...");
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Check if database exists and is accessible
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to database. Check connection string and SQL Server status.");
            throw new InvalidOperationException("Database connection failed");
        }

        // Apply pending migrations or create database
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        logger.LogInformation("✅ Database initialized successfully");

        // Seed initial data if needed
        await SeedInitialData(services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during database initialization: {Message}", ex.Message);

        if (app.Environment.IsDevelopment())
        {
            // In development, we can continue without database for some features
            logger.LogWarning("Continuing in development mode despite database error");
        }
        else
        {
            // In production, database is critical
            throw;
        }
    }
}

// -----------------------------------------------------
// 11. Configure HTTP Request Pipeline
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
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------
// 12. Configure Routes
// -----------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Account routes - simplified since we're using modals
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Index}",
    defaults: new { controller = "Account", action = "Index" });

// Add health check endpoint
app.MapGet("/health", async (ApplicationDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow })
                          : Results.Problem("Database connection failed");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Health check failed: {ex.Message}");
    }
});

// -----------------------------------------------------
// 13. Helper Methods
// -----------------------------------------------------
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

// -----------------------------------------------------
// 14. Start Application
// -----------------------------------------------------
//logger.LogInformation("Starting TidyUp application...");
app.Run();