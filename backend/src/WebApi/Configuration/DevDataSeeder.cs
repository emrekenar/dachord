namespace WebApi.Configuration;

using Application.Services;
using Domain.Interfaces;
using Domain.Models.User;

/// <summary>
/// Seeds a default admin account for local development so the admin/moderation
/// features can be exercised without manually promoting a user in DynamoDB.
/// Runs only in the Development environment; never in Lambda/production.
/// </summary>
public static class DevDataSeeder
{
    public const string AdminEmail = "admin@dachord.local";
    public const string AdminPassword = "admin1234";

    public static async Task SeedAsync(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        using var scope = app.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var logger = app.Logger;

        try
        {
            var existing = await userRepository.GetByEmailAsync(AdminEmail);
            if (existing is not null)
            {
                logger.LogInformation("Dev admin account already present ({Email})", AdminEmail);
                return;
            }

            var admin = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = AdminEmail,
                PasswordHash = SecurityService.HashPassword(AdminPassword),
                DisplayName = "Admin",
                Role = UserRole.Admin,
            };
            await userRepository.CreateUserAsync(admin);
            logger.LogInformation("Seeded dev admin account: {Email} / {Password}", AdminEmail, AdminPassword);
        }
        catch (Exception ex)
        {
            // Local DynamoDB may not be ready yet; don't crash startup over seeding.
            logger.LogWarning(ex, "Failed to seed dev admin account");
        }
    }
}
