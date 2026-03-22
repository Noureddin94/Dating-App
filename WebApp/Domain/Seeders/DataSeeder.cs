using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Domain.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var context = services.GetRequiredService<AppDbContext>();
        var randomUserService = services.GetRequiredService<IRandomUserService>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, context);
        await SeedFakeUsersAsync(userManager, context, randomUserService, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "ApprovedUser" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    private static async Task SeedAdminAsync(
        UserManager<IdentityUser> userManager, AppDbContext context)
    {
        const string email = "admin@datingapp.nl";
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(admin, "Admin@1234!");
        if (!result.Succeeded)
            throw new Exception($"Failed to seed admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(admin, "Admin");
        context.UserProfiles.Add(new UserProfile
        {
            UserId = admin.Id,
            FirstName = "App",
            LastName = "Admin",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Bio = "Platform administrator",
            City = "Amsterdam",
            Country = "Netherlands",
            Latitude = 52.3676,
            Longitude = 4.9041,
            Status = AccountStatus.Approved
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedFakeUsersAsync(
        UserManager<IdentityUser> userManager,
        AppDbContext context,
        IRandomUserService randomUserService,
        ILogger logger)
    {
        var existingApproved = context.UserProfiles.Count(p => p.Status == AccountStatus.Approved);
        if (existingApproved > 1)
        {
            logger.LogInformation("Skipping fake user seeding — {Count} approved users already exist.", existingApproved);
            return;
        }

        logger.LogInformation("Fetching 4 random users from randomuser.me...");
        var generatedUsers = await randomUserService.GetRandomUsersAsync(count: 4, nationality: "NL");

        if (!generatedUsers.Any())
        {
            logger.LogWarning("randomuser.me unreachable — falling back to hardcoded seed users.");
            await SeedFallbackUsersAsync(userManager, context);
            return;
        }

        foreach (var generated in generatedUsers)
        {
            if (await userManager.FindByEmailAsync(generated.Email) is not null) continue;

            var user = new IdentityUser { UserName = generated.Email, Email = generated.Email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "User@1234!");
            if (!result.Succeeded) { logger.LogWarning("Failed to seed {Email}", generated.Email); continue; }

            await userManager.AddToRoleAsync(user, "ApprovedUser");

            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = generated.FirstName,
                LastName = generated.LastName,
                DateOfBirth = generated.DateOfBirth,
                Gender = generated.Gender,
                City = generated.City,
                Country = generated.Country,
                Latitude = generated.Latitude,
                Longitude = generated.Longitude,
                Bio = $"Hi, I'm {generated.FirstName}! I'm from {generated.City}.",
                Status = AccountStatus.Approved
            };
            context.UserProfiles.Add(profile);
            await context.SaveChangesAsync();

            context.ProfileImages.Add(new ProfileImage
            {
                UserId = user.Id,
                BlobPath = generated.PictureUrl,
                IsPrimary = true,
                SortOrder = 0
            });
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Name} from {City}", $"{generated.FirstName} {generated.LastName}", generated.City);
        }
    }

    private static async Task SeedFallbackUsersAsync(
        UserManager<IdentityUser> userManager, AppDbContext context)
    {
        var fallback = new[]
        {
            ("emma@datingapp.nl",  "Emma",  "Bakker",   new DateOnly(1995, 3, 14), "Female", "Amsterdam", "Netherlands", 52.3676, 4.9041),
            ("liam@datingapp.nl",  "Liam",  "de Vries", new DateOnly(1993, 7, 22), "Male",   "Rotterdam", "Netherlands", 51.9225, 4.4792),
            ("sofia@datingapp.nl", "Sofia", "Jansen",   new DateOnly(1997, 11, 5), "Female", "Utrecht",   "Netherlands", 52.0907, 5.1214),
            ("noah@datingapp.nl",  "Noah",  "Smit",     new DateOnly(1991, 5, 30), "Male",   "The Hague", "Netherlands", 52.0705, 4.3007)
        };
        foreach (var (email, first, last, dob, gender, city, country, lat, lon) in fallback)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "User@1234!");
            if (!result.Succeeded) continue;
            await userManager.AddToRoleAsync(user, "ApprovedUser");
            context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FirstName = first,
                LastName = last,
                DateOfBirth = dob,
                Gender = gender,
                City = city,
                Country = country,
                Latitude = lat,
                Longitude = lon,
                Bio = $"Hi, I'm {first} from {city}.",
                Status = AccountStatus.Approved
            });
            await context.SaveChangesAsync();
        }
    }
}
