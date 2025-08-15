using TidyUpCapstone.Extensions;
using TidyUpCapstone.Helpers;
using TidyUpCapstone.Services;
using TidyUpCapstone.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure all services using extension methods
builder.Services
    .AddDatabaseServices(builder.Configuration)
    .AddIdentityServices()
    .AddTidyUpServices()
    .AddFileUploadConfiguration()
    .AddApiConfiguration()
    .AddLoggingConfiguration(builder.Environment)
    .AddCachingServices()
    .AddAIServices(builder.Configuration) // For future AI integration
    .AddBackgroundServices() // For future background tasks
    .AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<ITestUserHelper, TestUserHelper>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IViewModelService, ViewModelService>();

// ?? ADD SESSION SUPPORT
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add MVC and API controllers
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

var app = builder.Build();

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

// ?? ADD SESSION MIDDLEWARE (must be after UseRouting and before UseAuthentication)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Map routes
app.MapControllers(); // For API controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database in development
if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

app.Run();