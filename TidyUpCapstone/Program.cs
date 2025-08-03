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

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------
// 1. Database Configuration
// -----------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=(LocalDB)\\MSSQLLocalDB;Database=TidyUpdbCapstone;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
    ));

// -----------------------------------------------------
// 2. Identity Configuration (SSO Only)
// -----------------------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ "; // Added space at the end
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 0;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// -----------------------------------------------------
// 3. Application Cookie Config
// -----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// -----------------------------------------------------
// 4. External Authentication (Google + Placeholder Facebook)
// -----------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ExternalScheme;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    googleOptions.SaveTokens = false;
    googleOptions.Scope.Add("email");
    googleOptions.Scope.Add("profile");
});

/* Uncomment later for Facebook
.AddFacebook(facebookOptions =>
{
    facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
    facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
    facebookOptions.Scope.Add("email");
    facebookOptions.Fields.Add("email");
    facebookOptions.Fields.Add("name");
});
*/


// -----------------------------------------------------
// 5. Email Service Configuration (SendGrid)
// -----------------------------------------------------
// Configure SendGrid and Email settings
builder.Services.Configure<SendGridSettingsDto>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EmailSettings"));

// Register SendGrid client
builder.Services.AddSingleton<ISendGridClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<SendGridSettingsDto>>().Value;
    return new SendGridClient(settings.ApiKey);
});

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();


// -----------------------------------------------------
// 6. Add MVC
// -----------------------------------------------------
builder.Services.AddControllersWithViews();

var app = builder.Build();

// -----------------------------------------------------
// 6. Middleware
// -----------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();