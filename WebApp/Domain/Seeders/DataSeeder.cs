using Microsoft.AspNetCore.Identity;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Domain.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var context     = services.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, context);
        await SeedFakeUsersAsync(userManager, context);
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "ApprovedUser" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ── Admin user ────────────────────────────────────────────────────────────

    private static async Task SeedAdminAsync(
        UserManager<IdentityUser> userManager,
        AppDbContext context)
    {
        const string adminEmail    = "admin@datingapp.nl";
        const string adminPassword = "Admin@1234!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new IdentityUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
            throw new Exception($"Failed to seed admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(admin, "Admin");

        context.UserProfiles.Add(new UserProfile
        {
            UserId      = admin.Id,
            FirstName   = "App",
            LastName    = "Admin",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Bio         = "Platform administrator",
            Status      = AccountStatus.Approved
        });

        await context.SaveChangesAsync();
    }

    // ── Fake approved users ───────────────────────────────────────────────────

    private static async Task SeedFakeUsersAsync(
        UserManager<IdentityUser> userManager,
        AppDbContext context)
    {
        var fakeUsers = new[]
        {
            new FakeSeed("emma@datingapp.nl",   "Emma",   "Bakker",    new DateOnly(1995, 3, 14), "Female", "Amsterdam",  "Coffee lover, avid reader and weekend hiker."),
            new FakeSeed("liam@datingapp.nl",   "Liam",   "de Vries",  new DateOnly(1993, 7, 22), "Male",   "Rotterdam",  "Tech enthusiast, football fan, amateur chef."),
            new FakeSeed("sofia@datingapp.nl",  "Sofia",  "Jansen",    new DateOnly(1997, 11, 5), "Female", "Utrecht",    "Artist, cat person and hopeless romantic."),
            new FakeSeed("noah@datingapp.nl",   "Noah",   "Smit",      new DateOnly(1991, 5, 30), "Male",   "The Hague",  "Cyclist, bookworm, terrible at cooking.")
        };

        foreach (var seed in fakeUsers)
        {
            if (await userManager.FindByEmailAsync(seed.Email) is not null)
                continue;

            var user = new IdentityUser
            {
                UserName       = seed.Email,
                Email          = seed.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "User@1234!");
            if (!result.Succeeded)
                throw new Exception($"Failed to seed {seed.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, "ApprovedUser");

            context.UserProfiles.Add(new UserProfile
            {
                UserId      = user.Id,
                FirstName   = seed.FirstName,
                LastName    = seed.LastName,
                DateOfBirth = seed.DateOfBirth,
                Gender      = seed.Gender,
                Location    = seed.Location,
                Bio         = seed.Bio,
                Status      = AccountStatus.Approved
            });
        }

        await context.SaveChangesAsync();
    }

    private record FakeSeed(
        string Email,
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string Gender,
        string Location,
        string Bio);
}
