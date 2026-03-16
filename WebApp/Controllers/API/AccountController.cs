using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(UserManager<IdentityUser> userManager) 
        {
            _userManager = userManager;
        }


        [HttpGet]
        public IActionResult Welcome()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Ok("You are not authenticated.");
            }
            return Ok("Welcome, you are authenticated!");
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            var userProfile = new UserProfile
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = currentUser.UserName ?? "",
                LastName = currentUser.UserName ?? "",
            };

            return Ok(userProfile);
        }
    }
}