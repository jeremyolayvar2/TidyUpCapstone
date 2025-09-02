using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Services;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=(localdb)\\MSSQLLocalDB;Database=TidyUpdbSettings;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
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

// Configure cookie settings for better security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    // options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Register services
builder.Services.AddScoped<IPrivacyService, PrivacyService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddHttpContextAccessor();

// Add services to the container
builder.Services.AddControllersWithViews();

// Add antiforgery token configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Database seeding and initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        // Ensure database exists
        await context.Database.EnsureCreatedAsync();

        // Check if test user already exists
        var existingUser = await userManager.FindByEmailAsync("test@tidyup.com");
        if (existingUser == null)
        {
            var testUser = new AppUser
            {
                UserName = "testuser",
                Email = "test@tidyup.com",
                EmailConfirmed = true,
                DateCreated = DateTime.UtcNow,
                Status = "active"
            };

            var result = await userManager.CreateAsync(testUser, "Test123!");
            if (result.Succeeded)
            {
                // Create default privacy settings for the test user
                var privacySettings = new UserPrivacySettings
                {
                    UserId = testUser.Id,
                    ProfileVisibility = "public",
                    LocationVisibility = "show",
                    ActivityStreaksVisibility = "show",
                    OnlineStatus = "show",
                    SearchIndexing = "allow",
                    ContactVisibility = "public",
                    ActivityHistory = "show",
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                };

                context.UserPrivacySettings.Add(privacySettings);
                await context.SaveChangesAsync();

                Console.WriteLine("? Test user created successfully!");
                Console.WriteLine("Email: test@tidyup.com");
                Console.WriteLine("Password: Test123!");
                Console.WriteLine("? Default privacy settings created");
            }
            else
            {
                Console.WriteLine("? Failed to create test user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"- {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("? Test user already exists: test@tidyup.com");

            // Check if privacy settings exist for existing user
            var existingSettings = await context.UserPrivacySettings
                .FirstOrDefaultAsync(p => p.UserId == existingUser.Id);

            if (existingSettings == null)
            {
                var privacySettings = new UserPrivacySettings
                {
                    UserId = existingUser.Id,
                    ProfileVisibility = "public",
                    LocationVisibility = "show",
                    ActivityStreaksVisibility = "show",
                    OnlineStatus = "show",
                    SearchIndexing = "allow",
                    ContactVisibility = "public",
                    ActivityHistory = "show",
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                };

                context.UserPrivacySettings.Add(privacySettings);
                await context.SaveChangesAsync();
                Console.WriteLine("? Default privacy settings created for existing user");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error during initialization: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add this line for Account routes
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Index}",
    defaults: new { controller = "Account" });

app.Run();