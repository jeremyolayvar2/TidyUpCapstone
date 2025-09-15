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
// 3. Enhanced Identity Configuration for Modal System
// -----------------------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

    // Password settings - more secure for production, relaxed for development
    options.Password.RequireDigit = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireLowercase = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireUppercase = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireNonAlphanumeric = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequiredLength = builder.Environment.IsDevelopment() ? 3 : 8;
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
// 4. FIXED: Application Cookie Configuration FOR MODAL SYSTEM
// -----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    // FIXED: For modal-based authentication, keep these paths pointed to the home page
    // The modals will handle the UI, and we'll pass query parameters to trigger them
    options.LoginPath = "/"; // User goes to home page, modals handle login UI
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/";

    // Cookie settings
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    // FIXED: Add custom logic to handle unauthorized requests properly
    options.Events.OnRedirectToLogin = context =>
    {
        // If this is an AJAX request (from modals), return JSON instead of redirecting
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

        // For regular requests, redirect to home page with login modal trigger
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

    // FIXED: Handle access denied scenarios
    options.Events.OnRedirectToAccessDenied = context =>
    {
        // If this is an AJAX request, return JSON
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

        // For regular requests, redirect to home page
        context.Response.Redirect("/?error=access_denied");
        return Task.CompletedTask;
    };
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

        // FIXED: OAuth event handlers for modal system with enhanced error handling
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

            // FIXED: Redirect to home with modal parameters for modal system
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
            var contextLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            contextLogger.LogInformation("Facebook OAuth ticket created");
            return Task.CompletedTask;
        };
        
        facebookOptions.Events.OnRemoteFailure = context =>
        {
            var contextLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            contextLogger.LogError("Facebook OAuth remote failure: {Error}", context.Failure?.Message ?? "Unknown error");
            
            context.Response.Redirect("/?error=oauth_failed&showLogin=true");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    }
});
*/

// -----------------------------------------------------
// 6. Email Service Configuration (SendGrid)
// -----------------------------------------------------
builder.Services.Configure<SendGridSettingsDto>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EmailSettings"));

// Register SendGrid client with validation
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
    options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax for better OAuth compatibility
});

// -----------------------------------------------------
// 9. Build Application
// -----------------------------------------------------
var app = builder.Build();

// Get logger for this scope
var appLogger = app.Services.GetRequiredService<ILogger<Program>>();

// -----------------------------------------------------
// 10. Database Initialization with Better Error Handling
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
            // Apply pending migrations or create database
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                scopeLogger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
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
            // In development, we can continue without database for some features
            scopeLogger.LogWarning("Continuing in development mode despite database error");
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

    // FIXED: Add referrer policy for better OAuth compatibility
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

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

// FIXED: Account routes - simplified since we're using modals primarily
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Index}",
    defaults: new { controller = "Account", action = "Index" });

// FIXED: Add specific route for Main page
app.MapControllerRoute(
    name: "main",
    pattern: "Main",
    defaults: new { controller = "Home", action = "Main" });

// FIXED: Add specific routes for authentication actions
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

// FIXED: Add status endpoint for debugging authentication
//app.MapGet("/auth-status", async (HttpContext context, UserManager<AppUser> userManager) =>
//{
//    try
//    {
//        var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
//        var userId = userManager.GetUserId(context.User);
//        var userName = context.User?.Identity?.Name;

//        var claimsList = context.User?.Claims?.Select(c => new { c.Type, c.Value }).ToList();

//        return Results.Ok(new
//        {
//            isAuthenticated,
//            userId,
//            userName,
//            timestamp = DateTime.UtcNow,
//            claims = claimsList ?? new List<object>()
//        });
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"Auth status check failed: {ex.Message}");
//    }
//});

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
appLogger.LogInformation("TidyUp application starting...");
appLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
appLogger.LogInformation("Authentication configured with modal system");

app.Run();