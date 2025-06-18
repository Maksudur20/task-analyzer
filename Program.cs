using Microsoft.EntityFrameworkCore;
using Task_Analyzer.Data;
using Task_Analyzer; // Added this line
using Task_Analyzer.Models;
using Task_Analyzer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add after builder creation, before app.Build()
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString == null)
{
    throw new Exception("Connection string 'DefaultConnection' not found in configuration.");
}
try
{
    TestConnection.VerifyConnection(connectionString);
    
    // Check if Users table exists - if not, we may need to recreate the database
    if (!TestConnection.DoesUsersTableExist(connectionString))
    {
        Console.WriteLine("Users table not found. This may indicate the database was created without migrations.");
    }
    else
    {
        Console.WriteLine("Users table found. Database appears to be set up correctly.");
    }
}
catch (InvalidOperationException ex)
{
    // Log the more detailed exception from VerifyConnection
    // Consider whether to use a logger here if available, or rethrow.
    // For simplicity in startup, rethrowing might be appropriate.
    throw new Exception($"Database connection verification failed. {ex.Message}", ex);
}
catch (Exception ex) // Catch any other unexpected exceptions during verification
{
    throw new Exception($"An unexpected error occurred during database connection verification. Connection string: {connectionString}", ex);
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Add detailed error handling for database initialization
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the database.");
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database with roles and admin user
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Starting database initialization");
        await DbInitializer.Initialize(services, logger as ILogger);
        logger.LogInformation("Database initialization completed successfully");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding the database.");
}

app.Run();
