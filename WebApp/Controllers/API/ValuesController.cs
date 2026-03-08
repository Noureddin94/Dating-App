using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.API
{

    [ApiController]
    [Route("api/[controller]")]
    public class ValuesssssController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Hotel retrieved successfully",
                data = new
                {
                    Id = 1,
                    Name = "Hotel California",
                    Location = "Los Angeles"
                }
            });
        }
    }
}
