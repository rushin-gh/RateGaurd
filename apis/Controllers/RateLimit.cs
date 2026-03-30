using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimit : ControllerBase
    {
        [HttpGet("~/api/test")]
        public IActionResult Test([Required] int userId)
        {
            return Ok($"API working {userId}");
        }
    }
}
