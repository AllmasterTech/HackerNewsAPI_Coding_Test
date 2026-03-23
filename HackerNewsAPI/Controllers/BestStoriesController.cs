using HackerNewsAPI.Models;
using HackerNewsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BestStoriesController : ControllerBase
{
    private readonly IHackerNewsService _hackerNewsService;
    private readonly ILogger<BestStoriesController> _logger;

    public BestStoriesController(
        IHackerNewsService hackerNewsService,
        ILogger<BestStoriesController> logger)
    {
        _hackerNewsService = hackerNewsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<StoryResponse>>> GetBestStories([FromQuery] int n = 10)
    {
        try
        {
            if (n <= 0)
            {
                return BadRequest(new { error = "n must be greater than 0" });
            }

            var stories = await _hackerNewsService.GetBestStoriesAsync(n);
            return Ok(stories);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving best stories");
            return StatusCode(500, new { error = "An error occurred while retrieving stories" });
        }
    }
}

