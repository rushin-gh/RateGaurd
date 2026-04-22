using apis.Contracts;
using apis.Model;
using apis.Scripts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimitController : ControllerBase
    {
        [HttpGet("~/api/test")]
        public async Task<IActionResult> Test()
        {
            return Ok($"Test API working");
        }
    }
}
