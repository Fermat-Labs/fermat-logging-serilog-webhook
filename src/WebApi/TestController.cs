using Fermat.Exceptions.Core.Types;
using Microsoft.AspNetCore.Mvc;

namespace WebApi;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet("log")]
    public IActionResult LogMessage()
    {
        _logger.LogInformation("This is a test log message from TestController.");
        return Ok("Log message sent.");
    }
    
    [HttpGet("error")]
    public IActionResult LogError()
    {
        try
        {
            try
            {
                throw new InvalidOperationException("This is a test exception.");
            }
            catch (Exception e)
            {
                throw new AppServiceUnavailableException("Service is unavailable.", e);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in TestController.");
            return StatusCode(500, "An error occurred and has been logged.");
        }
    }
}