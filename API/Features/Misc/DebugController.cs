using DotNetAngularTemplate.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAngularTemplate.Features.Misc;

[ApiController]
[Route("api/v1/debug")]
public class DebugController : ControllerBase
{
    [HttpGet]
    [Route("ip")]
    public IActionResult Ip()
    {
        return Ok(new
        {
            ip = IpHelper.GetClientIp(HttpContext)
        });
    }
}