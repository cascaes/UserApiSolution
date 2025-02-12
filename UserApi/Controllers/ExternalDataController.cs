using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ExternalDataController : ControllerBase
{
    private readonly IExternalApiService _externalApiService;

    public ExternalDataController(IExternalApiService externalApiService)
    {
        _externalApiService = externalApiService;
    }

    [HttpGet("posts")]
    public async Task<ActionResult<string>> GetPosts()
    {
        var posts = await _externalApiService.GetPostsAsync();
        return Ok(posts);
    }
}