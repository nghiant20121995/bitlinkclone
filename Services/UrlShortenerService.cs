using MongoDB.Driver;
using UrlShortener.Models;
using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.Services;

public class UrlShortenerService
{
    private readonly IMongoCollection<ShortenedUrl> _urlCollection;
    private readonly string _baseUrl;

    public UrlShortenerService(IMongoDatabase database, IConfiguration configuration)
    {
        _urlCollection = database.GetCollection<ShortenedUrl>("urls");
        _baseUrl = configuration["BaseUrl"] ?? "https://localhost:7001";
    }

    public async Task<CreateUrlResponse> CreateShortUrlAsync(string originalUrl)
    {
        // Check if URL already exists
        var existingUrl = await _urlCollection
            .Find(u => u.OriginalUrl == originalUrl)
            .FirstOrDefaultAsync();

        if (existingUrl != null)
        {
            return new CreateUrlResponse
            {
                OriginalUrl = existingUrl.OriginalUrl,
                ShortCode = existingUrl.ShortCode,
                ShortenedUrl = $"{_baseUrl}/{existingUrl.ShortCode}",
                CreatedAt = existingUrl.CreatedAt
            };
        }

        // Generate unique short code
        var shortCode = await GenerateUniqueShortCodeAsync();

        // Create new shortened URL
        var shortenedUrl = new ShortenedUrl
        {
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow
        };

        await _urlCollection.InsertOneAsync(shortenedUrl);

        return new CreateUrlResponse
        {
            OriginalUrl = shortenedUrl.OriginalUrl,
            ShortCode = shortenedUrl.ShortCode,
            ShortenedUrl = $"{_baseUrl}/{shortenedUrl.ShortCode}",
            CreatedAt = shortenedUrl.CreatedAt
        };
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var shortenedUrl = await _urlCollection
            .Find(u => u.ShortCode == shortCode)
            .FirstOrDefaultAsync();

        if (shortenedUrl != null)
        {
            // Increment click count
            await _urlCollection.UpdateOneAsync(
                u => u.ShortCode == shortCode,
                Builders<ShortenedUrl>.Update.Inc(u => u.ClickCount, 1)
            );
            
            return shortenedUrl.OriginalUrl;
        }

        return null;
    }

    private async Task<string> GenerateUniqueShortCodeAsync()
    {
        string shortCode;
        bool exists;

        do
        {
            shortCode = GenerateShortCode();
            exists = await _urlCollection
                .Find(u => u.ShortCode == shortCode)
                .AnyAsync();
        } while (exists);

        return shortCode;
    }

    private static string GenerateShortCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[6];
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(result);
    }

    public async Task CreateIndexesAsync()
    {
        // Create index on shortCode for faster lookups
        var shortCodeIndex = Builders<ShortenedUrl>.IndexKeys.Ascending(x => x.ShortCode);
        await _urlCollection.Indexes.CreateOneAsync(new CreateIndexModel<ShortenedUrl>(shortCodeIndex));

        // Create index on originalUrl for faster duplicate checks
        var originalUrlIndex = Builders<ShortenedUrl>.IndexKeys.Ascending(x => x.OriginalUrl);
        await _urlCollection.Indexes.CreateOneAsync(new CreateIndexModel<ShortenedUrl>(originalUrlIndex));
    }
} 