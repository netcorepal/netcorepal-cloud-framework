using Microsoft.AspNetCore.Mvc;

namespace NetCorePal.Web.Controllers;

/// <summary>
/// Test controller for source generator
/// </summary>
[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test action method
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Test");
    }

    /// <summary>
    /// Another test method
    /// </summary>
    [HttpPost]
    public IActionResult Post([FromBody] string value)
    {
        return Ok($"Received: {value}");
    }
}
