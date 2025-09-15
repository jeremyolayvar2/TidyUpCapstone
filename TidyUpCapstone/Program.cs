using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using SendGrid;
using Microsoft.Extensions.Options;
using TidyUpCapstone.Models.DTOs.Configuration;
using SendGrid.Helpers.Mail;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------
// 1. Enhanced Logging Configuration
// -----------------------------------------------------
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();

    if (builder.Environment.IsDevelopment())
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
    else
    {
        logging.SetMinimumLevel(LogLevel.Information);
    }
});

// -----------------------------------------------------
// 2. Database Configuration
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
// 3. Identity Configuration for Modal System
// -----------------------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

    // Password settings
    options.Password.RequireDigit = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireLowercase = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireUppercase = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequireNonAlphanumeric = builder.Environment.IsDevelopment() ? false : true;
    options.Password.RequiredLength = builder.Environment.IsDevelopment() ? 3 : 8;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// -----------------------------------------------------
// 4. Application Cookie Configuration FOR MODAL SYSTEM
// -----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    // CRITICAL: Redirect to home page instead of Login views
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
});

// -----------------------------------------------------
// 5. External Authentication Configuration
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
        logger.LogWarning("Google OAuth credentials are not properly configured");
    }
    else
    {
        googleOptions.ClientId = clientId;
        googleOptions.ClientSecret = clientSecret;
        googleOptions.SaveTokens = true;
        googleOptions.Scope.Add("email");
        googleOptions.Scope.Add("profile");

        // OAuth event handlers for modal system
        googleOptions.Events.OnRemoteFailure = context =>
        {
            var errorMessage = "oauth_failed";
            if (context.Failure?.Message?.Contains("access_denied") == true)
            {
                errorMessage = "access_denied";
            }

            // Redirect to home with modal parameters
            context.Response.Redirect($"/?error={errorMessage}&showLogin=true");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    }
});

// -----------------------------------------------------
// 6. Email Service Configuration
// -----------------------------------------------------
builder.Services.Configure<SendGridSettingsDto>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddSingleton<ISendGridClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<SendGridSettingsDto>>().Value;
    return new SendGridClient(settings.ApiKey ?? "");
});

builder.Services.AddScoped<IEmailService, EmailService>();

//// Add language service if it exists
//try
//{
//    builder.Services.AddScoped<ILanguageService, LanguageService>();
//}
//catch
//{
//    // Service might not exist - that's fine
//}

// -----------------------------------------------------
// 7. MVC Configuration
// -----------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    if (!builder.Environment.IsDevelopment())
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequireHttpsAttribute());
    }
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// -----------------------------------------------------
// 8. Build Application
// -----------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------
// 9. Database Initialization
// -----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing database...");
        var context = services.GetRequiredService<ApplicationDbContext>();

        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to database");
            if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Database connection failed");
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("✅ Database initialized successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Database initialization error: {Message}", ex.Message);
        if (!app.Environment.IsDevelopment()) throw;
    }
}

// -----------------------------------------------------
// 10. Configure HTTP Pipeline
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
// 11. Configure Routes
// -----------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Health check endpoint
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

app.Run();