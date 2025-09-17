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
// 2. Configure Core Services using Extension Methods (from feature/item-management)
// -----------------------------------------------------
builder.Services
    .AddDatabaseServices(builder.Configuration)
    .AddIdentityServices()  // This already configures Identity - don't duplicate
    .AddTidyUpServices()
    .AddFileUploadConfiguration()
    .AddApiConfiguration()
    .AddLoggingConfiguration(builder.Environment)
    .AddCachingServices()
    .AddAIServices(builder.Configuration)
    .AddBackgroundServices();

// -----------------------------------------------------
// 3. Register Item Management Services (from feature/item-management)
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
// 4. REMOVED: Duplicate Identity Configuration
// -----------------------------------------------------
// This section was removed because .AddIdentityServices() already handles it
// Keeping this would cause "Scheme already exists: Identity.Application" error

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
// 8. Google Cloud and Vision Services Configuration (from feature/item-management)
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
// 9. Session Support (from feature/item-management)
// -----------------------------------------------------
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
// 13. Configure HTTP Request Pipeline (merged)
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

    // FIXED: Add referrer policy for better OAuth compatibility
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
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
// 14. Configure Routes (merged)
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

// Initialize database in development using extension method (from feature/item-management)
if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

// -----------------------------------------------------
// 15. Helper Methods (from dev)
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
// 16. Start Application
// -----------------------------------------------------
appLogger.LogInformation("TidyUp application starting...");
appLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
appLogger.LogInformation("Authentication configured with modal system");
appLogger.LogInformation("Item management features enabled");

app.Run();