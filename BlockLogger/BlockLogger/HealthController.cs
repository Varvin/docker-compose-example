using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BlockLogger
{
    [Route("api/v1/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet("ping")]
        public Task Ping()
        {
            return Task.CompletedTask;
        }
    }
}