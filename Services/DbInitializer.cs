using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Analyzer.Data;
using Task_Analyzer.Models;

namespace Task_Analyzer.Services
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                
                // Create database if it doesn't exist
                context.Database.Migrate();
                
                logger.LogInformation("Database migration complete");
                
                // Seed Roles
                await SeedRoles(roleManager);
                logger.LogInformation("Roles seeded");
                
                // Seed Admin User
                await SeedAdminUser(userManager);
                logger.LogInformation("Admin user seeded");
                
                // Update registration dates for existing users with default dates
                await UpdateRegistrationDates(context);
                logger.LogInformation("Registration dates updated");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
        
        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };
            
            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        
        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            // Create admin user if it doesn't exist
            var adminEmail = "admin@taskanalyzer.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    RegistrationDate = DateTime.Now
                };
                
                // Use a secure password in production
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
        
        private static async Task UpdateRegistrationDates(ApplicationDbContext context)
        {
            // Get users with default registration date (01/01/0001)
            var usersWithDefaultDate = await context.Users
                .Where(u => u.RegistrationDate.Year < 2000)
                .ToListAsync();
            
            if (usersWithDefaultDate.Any())
            {
                foreach (var user in usersWithDefaultDate)
                {                    
                    // Set to current date if it's the default date
                    user.RegistrationDate = DateTime.Now;
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
}
