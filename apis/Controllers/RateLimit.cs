using apis.Contracts;
using apis.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimit : ControllerBase
    {
        private readonly IRateLimitService rateLimitService;

        public RateLimit(IRateLimitService rateLimitService)
        {
            this.rateLimitService = rateLimitService;
        }

        [HttpGet("~/api/test")]
        public IActionResult Test([Required] int userId)
        {
            var limitResult = rateLimitService.GetRateLimitResult(userId);
            if (!limitResult.IsAllowed)
            {
                Response.Headers["Retry-After"] = limitResult.TimesInSeconds.ToString();
                return StatusCode(429, $"Too much requests for {userId}");
            }

            return Ok($"API working {userId}");
        }
    }
}
