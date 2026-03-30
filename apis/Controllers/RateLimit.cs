using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimit : ControllerBase
    {
        [HttpGet("~/api/test")]
        public IActionResult Test()
        {
            return Ok("API working");
        }
    }
}
