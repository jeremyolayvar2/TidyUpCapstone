using TidyUpCapstone.Extensions;

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
    .AddBackgroundServices(); // For future background tasks

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