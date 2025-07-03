namespace UrlShortener.Models;

public class CreateUrlResponse
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string ShortenedUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
} 