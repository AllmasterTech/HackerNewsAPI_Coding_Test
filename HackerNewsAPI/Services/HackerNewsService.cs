using System.Text.Json;
using HackerNewsAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HackerNewsAPI.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HackerNewsService> _logger;
    private readonly HackerNewsOptions _options;

    public HackerNewsService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<HackerNewsService> logger,
        IOptions<HackerNewsOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<List<StoryResponse>> GetBestStoriesAsync(int n)
    {
        if (n <= 0)
        {
            throw new ArgumentException("n must be greater than 0", nameof(n));
        }

        if (n > _options.MaxStoriesLimit)
        {
            throw new ArgumentException($"n cannot exceed {_options.MaxStoriesLimit}", nameof(n));
        }

        // Get best story IDs (cached)
        var storyIds = await GetBestStoryIdsAsync();

        // Fetch story details in parallel (with caching)
        var storyTasks = storyIds
            .Take(_options.MaxStoriesLimit)
            .Select(id => GetStoryDetailsAsync(id));

        var stories = await Task.WhenAll(storyTasks);

        // Filter out null stories and sort by score descending
        var validStories = stories
            .OfType<HackerNewsStory>()
            .OrderByDescending(s => s.Score)
            .Take(n)
            .Select(MapToResponse)
            .ToList();

        return validStories;
    }

    private async Task<List<int>> GetBestStoryIdsAsync()
    {
        const string cacheKey = "beststories:ids";

        if (_cache.TryGetValue(cacheKey, out List<int>? cachedIds))
        {
            return cachedIds ?? new List<int>();
        }

        try
        {
            var response = await _httpClient.GetStringAsync($"{_options.BaseUrl}/beststories.json");
            var ids = JsonSerializer.Deserialize<List<int>>(response) ?? new List<int>();

            _cache.Set(cacheKey, ids, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes),
                Size = 1
            });

            return ids;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch best story IDs");
            throw;
        }
    }

    private async Task<HackerNewsStory?> GetStoryDetailsAsync(int storyId)
    {
        var cacheKey = $"beststories:item:{storyId}";

        if (_cache.TryGetValue(cacheKey, out HackerNewsStory? cachedStory))
        {
            return cachedStory;
        }

        try
        {
            var response = await _httpClient.GetStringAsync($"{_options.BaseUrl}/item/{storyId}.json");
            var story = JsonSerializer.Deserialize<HackerNewsStory>(response);

            if (story != null)
            {
                _cache.Set(cacheKey, story, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes),
                    Size = 1
                });
            }

            return story;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch story {StoryId}", storyId);
            return null;
        }
    }

    private static StoryResponse MapToResponse(HackerNewsStory story)
    {
        var commentCount = story.Descendants ?? story.Kids?.Count ?? 0;
        var time = DateTimeOffset.FromUnixTimeSeconds(story.Time).ToString("yyyy-MM-ddTHH:mm:ss+00:00");

        return new StoryResponse
        {
            Title = story.Title ?? string.Empty,
            Uri = story.Url ?? string.Empty,
            PostedBy = story.By ?? string.Empty,
            Time = time,
            Score = story.Score,
            CommentCount = commentCount
        };
    }
}

public class HackerNewsOptions
{
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0";
    public int CacheExpirationMinutes { get; set; } = 5;
    public int MaxStoriesLimit { get; set; } = 500;
}
