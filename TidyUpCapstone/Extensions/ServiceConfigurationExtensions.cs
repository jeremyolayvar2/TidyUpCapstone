using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Services;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Extensions
{
    public static class ServiceConfigurationExtensions
    {
        /// <summary>
        /// Configure database services
        /// </summary>
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection") ??
                    "Server=OLAYVAR\\SQLEXPRESS;Database=TidyUpdb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
                ));

            return services;
        }

        /// <summary>
        /// Configure Identity services
        /// </summary>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole<int>>(options =>
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

            return services;
        }

        /// <summary>
        /// Configure TidyUp application services
        /// </summary>
        public static IServiceCollection AddTidyUpServices(this IServiceCollection services)
        {
            // Core services
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IItemService, ItemService>();

            // Add more services here as you create them
            // services.AddScoped<ITransactionService, TransactionService>();
            // services.AddScoped<IUserService, UserService>();
            // services.AddScoped<INotificationService, NotificationService>();

            return services;
        }

        /// <summary>
        /// Configure file upload and form options
        /// </summary>
        public static IServiceCollection AddFileUploadConfiguration(this IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
                options.KeyLengthLimit = int.MaxValue;
            });

            return services;
        }

        /// <summary>
        /// Configure API behavior and validation
        /// </summary>
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage);

                    return new BadRequestObjectResult(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = errors
                    });
                };
            });

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = true;
            });

            return services;
        }

        /// <summary>
        /// Configure logging services
        /// </summary>
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();

                if (environment.IsDevelopment())
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                }
                else
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                }
            });

            return services;
        }

        /// <summary>
        /// Configure caching services
        /// </summary>
        public static IServiceCollection AddCachingServices(this IServiceCollection services)
        {
            services.AddMemoryCache();

            // Add distributed cache if needed for production
            // services.AddStackExchangeRedisCache(options =>
            // {
            //     options.Configuration = "your-redis-connection-string";
            // });

            return services;
        }

        /// <summary>
        /// Configure AI services (placeholder for future implementation)
        /// </summary>
        public static IServiceCollection AddAIServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Placeholder for Azure Computer Vision and TensorFlow services
            // services.Configure<AzureComputerVisionOptions>(configuration.GetSection("AzureComputerVision"));
            // services.Configure<TensorFlowOptions>(configuration.GetSection("TensorFlow"));

            // services.AddScoped<IAzureComputerVisionService, AzureComputerVisionService>();
            // services.AddScoped<ITensorFlowService, TensorFlowService>();
            // services.AddScoped<IAIAnalysisService, AIAnalysisService>();

            return services;
        }

        /// <summary>
        /// Configure background services
        /// </summary>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // Add background services for AI processing, cleanup, etc.
            // services.AddHostedService<AIProcessingBackgroundService>();
            // services.AddHostedService<FileCleanupBackgroundService>();
            // services.AddHostedService<ExpiredItemsCleanupService>();

            return services;
        }

        /// <summary>
        /// Initialize database and seed data
        /// </summary>
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database initialized successfully");

                // Seed initial data if needed
                await SeedInitialDataAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        /// <summary>
        /// Seed initial data for development
        /// </summary>
        private static async Task SeedInitialDataAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // Seed categories if they don't exist
                if (!context.ItemCategories.Any())
                {
                    var categories = new[]
                    {
                        new Models.Entities.Items.ItemCategory { Name = "Books & Stationery", IsActive = true, SortOrder = 1 },
                        new Models.Entities.Items.ItemCategory { Name = "Electronics & Gadgets", IsActive = true, SortOrder = 2 },
                        new Models.Entities.Items.ItemCategory { Name = "Toys & Games", IsActive = true, SortOrder = 3 },
                        new Models.Entities.Items.ItemCategory { Name = "Home & Kitchen", IsActive = true, SortOrder = 4 },
                        new Models.Entities.Items.ItemCategory { Name = "Furniture", IsActive = true, SortOrder = 5 },
                        new Models.Entities.Items.ItemCategory { Name = "Appliances", IsActive = true, SortOrder = 6 },
                        new Models.Entities.Items.ItemCategory { Name = "Health & Beauty", IsActive = true, SortOrder = 7 },
                        new Models.Entities.Items.ItemCategory { Name = "Crafts & DIY", IsActive = true, SortOrder = 8 },
                        new Models.Entities.Items.ItemCategory { Name = "School & Office", IsActive = true, SortOrder = 9 },
                        new Models.Entities.Items.ItemCategory { Name = "Sentimental Items", IsActive = true, SortOrder = 10 },
                        new Models.Entities.Items.ItemCategory { Name = "Miscellaneous", IsActive = true, SortOrder = 11 }
                    };

                    context.ItemCategories.AddRange(categories);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {Count} item categories", categories.Length);
                }

                // Seed conditions if they don't exist
                if (!context.ItemConditions.Any())
                {
                    var conditions = new[]
                    {
                        new Models.Entities.Items.ItemCondition { Name = "Brand New", ConditionMultiplier = 1.25m, IsActive = true },
                        new Models.Entities.Items.ItemCondition { Name = "Like New", ConditionMultiplier = 1.15m, IsActive = true },
                        new Models.Entities.Items.ItemCondition { Name = "Gently Used", ConditionMultiplier = 1.05m, IsActive = true },
                        new Models.Entities.Items.ItemCondition { Name = "Visible Wear", ConditionMultiplier = 0.90m, IsActive = true },
                        new Models.Entities.Items.ItemCondition { Name = "For Repair/Parts", ConditionMultiplier = 0.75m, IsActive = true }
                    };

                    context.ItemConditions.AddRange(conditions);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {Count} item conditions", conditions.Length);
                }

                // Seed common locations if they don't exist
                if (!context.ItemLocations.Any())
                {
                    var locations = new[]
                    {
                        new Models.Entities.Items.ItemLocation { Name = "Marikina City", Region = "Metro Manila", IsActive = true },
                        new Models.Entities.Items.ItemLocation { Name = "Quezon City", Region = "Metro Manila", IsActive = true },
                        new Models.Entities.Items.ItemLocation { Name = "Manila", Region = "Metro Manila", IsActive = true },
                        new Models.Entities.Items.ItemLocation { Name = "Pasig City", Region = "Metro Manila", IsActive = true },
                        new Models.Entities.Items.ItemLocation { Name = "Mandaluyong City", Region = "Metro Manila", IsActive = true }
                    };

                    context.ItemLocations.AddRange(locations);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {Count} item locations", locations.Length);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while seeding initial data");
                throw;
            }
        }
    }
}