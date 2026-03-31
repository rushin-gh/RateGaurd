using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimit : ControllerBase
    {
        static ConcurrentDictionary<int, (List<DateTime> TimeStamps, object Lock)> userRequests = new();


        [HttpGet("~/api/test")]
        public IActionResult Test([Required] int userId)
        {
            var entry = userRequests.GetOrAdd(userId, _ => (new List<DateTime>(), new object()));
            lock (entry.Lock)
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-1);

                // Clean up old timestamps (fixes memory leak too)
                entry.TimeStamps.RemoveAll(d => d < cutoff);

                if (entry.TimeStamps.Count >= 5)
                {
                    Response.Headers["Retry-After"] = "60";
                    return StatusCode(429);
                }

                entry.TimeStamps.Add(DateTime.UtcNow);
            }

            return Ok($"API working {userId}");


            //if (userRequests.ContainsKey(userId))
            //    userRequests[userId].RemoveAll(item => item < DateTime.Now.AddMinutes(-1));

            //int reqCnt = 0;
            //if (userRequests.ContainsKey(userId))
            //{
            //    reqCnt = userRequests[userId].Count();   
            //}

            //if (reqCnt >= 5)
            //{
            //    return StatusCode(429);
            //}

            //if (userRequests.ContainsKey(userId))
            //{
            //    userRequests[userId].Add(DateTime.Now);      
            //} 
            //else
            //{
            //    userRequests.Add(userId, new List<DateTime>() { DateTime.Now });
            //}

            ////foreach (var item in userRequests)
            ////{
            ////    Console.WriteLine($"User  {item.Key}");
            ////    foreach(var litem in item.Value)
            ////    {
            ////        Console.WriteLine(litem);
            ////    }
            ////}
            ////Console.WriteLine();

            //return Ok($"API working {userId}");
        }
    }
}
