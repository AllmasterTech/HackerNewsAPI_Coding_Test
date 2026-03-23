using HackerNewsAPI.Models;

namespace HackerNewsAPI.Services;

public interface IHackerNewsService
{
    Task<List<StoryResponse>> GetBestStoriesAsync(int n);
}

