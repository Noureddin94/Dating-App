using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User is not authenticated.");

    protected IActionResult HandleException(Exception ex) => ex switch
    {
        KeyNotFoundException     => NotFound(new { error = ex.Message }),
        UnauthorizedAccessException => Forbid(),
        InvalidOperationException   => BadRequest(new { error = ex.Message }),
        _                           => StatusCode(500, new { error = "An unexpected error occurred." })
    };
}
