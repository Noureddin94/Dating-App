using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Entities;
using WebApp.Domain.Enums;
using WebApp.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;

namespace WebApp.Controllers.API;

[Authorize(Policy = "AdminOnly")]
public class RandomUserController(
    IRandomUserService randomUserService,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext context) : BaseApiController
{
    // POST api/randomuser/seed?count=5&gender=female&nationality=NL
    // Fetches users from randomuser.me and saves them to the database
    [HttpPost("seed")]
    public async Task<IActionResult> SeedRandomUsers(
        [FromQuery] int count = 5,
        [FromQuery] string? gender = null,
        [FromQuery] string? nationality = null)
    {
        try
        {
            if (count < 1 || count > 50)
                return BadRequest(new { error = "Count must be between 1 and 50." });

            var generatedUsers = await randomUserService
                .GetRandomUsersAsync(count, gender, nationality);

            if (!generatedUsers.Any())
                return StatusCode(503, new
                {
                    error = "Could not fetch users from randomuser.me. Try again."
                });

            var created  = new List<object>();
            var skipped  = new List<string>();

            // Ensure ApprovedUser role exists
            if (!await roleManager.RoleExistsAsync("ApprovedUser"))
                await roleManager.CreateAsync(new IdentityRole("ApprovedUser"));

            foreach (var generated in generatedUsers)
            {
                // Skip if email already exists
                if (await userManager.FindByEmailAsync(generated.Email) is not null)
                {
                    skipped.Add(generated.Email);
                    continue;
                }

                var identityUser = new IdentityUser
                {
                    UserName       = generated.Email,
                    Email          = generated.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(identityUser, "User@1234!");
                if (!result.Succeeded)
                {
                    skipped.Add(generated.Email);
                    continue;
                }

                await userManager.AddToRoleAsync(identityUser, "ApprovedUser");

                // Store picture URL as blob path (real photo from randomuser.me)
                var profile = new UserProfile
                {
                    UserId      = identityUser.Id,
                    FirstName   = generated.FirstName,
                    LastName    = generated.LastName,
                    DateOfBirth = generated.DateOfBirth,
                    Gender      = generated.Gender,
                    City        = generated.City,
                    Country     = generated.Country,
                    Latitude    = generated.Latitude,
                    Longitude   = generated.Longitude,
                    Bio         = $"Hi, I'm {generated.FirstName}! I'm from {generated.City}.",
                    Status      = AccountStatus.Approved
                };

                context.UserProfiles.Add(profile);
                await context.SaveChangesAsync();

                // Save profile picture URL from randomuser.me as a ProfileImage
                context.ProfileImages.Add(new ProfileImage
                {
                    UserId    = identityUser.Id,
                    BlobPath  = generated.PictureUrl,
                    IsPrimary = true,
                    SortOrder = 0
                });

                await context.SaveChangesAsync();

                created.Add(new
                {
                    userId     = identityUser.Id,
                    email      = generated.Email,
                    name       = $"{generated.FirstName} {generated.LastName}",
                    city       = generated.City,
                    country    = generated.Country,
                    age        = generated.Age,
                    gender     = generated.Gender,
                    pictureUrl = generated.PictureUrl
                });
            }

            return Ok(new
            {
                message      = $"Seeded {created.Count} users from randomuser.me",
                created,
                skippedCount = skipped.Count,
                skipped
            });
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/randomuser/preview?count=3&gender=male&nationality=NL
    // Fetches from the API and returns the data WITHOUT saving to the database
    // Great for demonstrating the live API call in Swagger
    [HttpGet("preview")]
    public async Task<IActionResult> PreviewRandomUsers(
        [FromQuery] int count = 3,
        [FromQuery] string? gender = null,
        [FromQuery] string? nationality = null)
    {
        try
        {
            if (count < 1 || count > 20)
                return BadRequest(new { error = "Count must be between 1 and 20." });

            var users = await randomUserService
                .GetRandomUsersAsync(count, gender, nationality);

            if (!users.Any())
                return StatusCode(503, new
                {
                    error = "Could not reach randomuser.me. Check your connection."
                });

            return Ok(new
            {
                source  = "https://randomuser.me",
                fetched = users.Count,
                users   = users.Select(u => new
                {
                    name       = $"{u.FirstName} {u.LastName}",
                    email      = u.Email,
                    gender     = u.Gender,
                    age        = u.Age,
                    city       = u.City,
                    country    = u.Country,
                    coordinates = new { u.Latitude, u.Longitude },
                    pictureUrl = u.PictureUrl
                })
            });
        }
        catch (Exception ex) { return HandleException(ex); }
    }

    // GET api/randomuser/nationalities
    // Shows the nationality codes the API supports
    [HttpGet("nationalities")]
    public IActionResult GetNationalities()
    {
        var nationalities = new[]
        {
            new { code = "AU", country = "Australia" },
            new { code = "BR", country = "Brazil" },
            new { code = "CA", country = "Canada" },
            new { code = "CH", country = "Switzerland" },
            new { code = "DE", country = "Germany" },
            new { code = "DK", country = "Denmark" },
            new { code = "ES", country = "Spain" },
            new { code = "FI", country = "Finland" },
            new { code = "FR", country = "France" },
            new { code = "GB", country = "United Kingdom" },
            new { code = "IE", country = "Ireland" },
            new { code = "IN", country = "India" },
            new { code = "IR", country = "Iran" },
            new { code = "MX", country = "Mexico" },
            new { code = "NL", country = "Netherlands" },
            new { code = "NO", country = "Norway" },
            new { code = "NZ", country = "New Zealand" },
            new { code = "RS", country = "Serbia" },
            new { code = "TR", country = "Turkey" },
            new { code = "UA", country = "Ukraine" },
            new { code = "US", country = "United States" }
        };

        return Ok(new
        {
            documentation = "https://randomuser.me/documentation#nationalities",
            nationalities
        });
    }
}
