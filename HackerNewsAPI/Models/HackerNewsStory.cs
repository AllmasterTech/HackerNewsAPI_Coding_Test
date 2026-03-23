using System.Text.Json.Serialization;

namespace HackerNewsAPI.Models;

public class HackerNewsStory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("by")]
    public string? By { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("descendants")]
    public int? Descendants { get; set; }

    [JsonPropertyName("kids")]
    public List<int>? Kids { get; set; }
}
