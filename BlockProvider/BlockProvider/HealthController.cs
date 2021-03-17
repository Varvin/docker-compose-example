using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BlockProvider
{
    [Route("api/v1/[controller]")]
    public class HealthController:ControllerBase
    {
        [HttpGet("ping")]
        public Task Ping()
        {
            return Task.CompletedTask;
        }
    }
}