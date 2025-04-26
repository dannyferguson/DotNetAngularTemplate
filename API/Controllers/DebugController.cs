using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAngularTemplate.Controllers;

[ApiController]
[Route("api/v1/debug")]
public class DebugController : ControllerBase
{
    [HttpGet]
    [Route("ip")]
    public IActionResult Ip()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
    
        if (string.IsNullOrEmpty(ip))
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        // Nobody cares about IPv6
        if (ip == IPAddress.IPv6Loopback.ToString())
        {
            ip = IPAddress.Loopback.ToString();
        }

        return Ok(new { ip });
    }
}