using MongoDB.Driver;
using UrlShortener.Models;
using UrlShortener.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("MongoDB") ?? configuration["MongoDB:ConnectionString"];
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var databaseName = configuration["MongoDB:DatabaseName"];
    return client.GetDatabase(databaseName);
});

builder.Services.AddSingleton<UrlShortenerService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Initialize MongoDB indexes
using (var scope = app.Services.CreateScope())
{
    var urlService = scope.ServiceProvider.GetRequiredService<UrlShortenerService>();
    await urlService.CreateIndexesAsync();
}

// API Endpoints

// POST /api/urls - Create a shortened URL
app.MapPost("/api/urls", async ([FromBody] CreateUrlRequest request, UrlShortenerService urlService) =>
{
    var validationContext = new ValidationContext(request);
    var validationResults = new List<ValidationResult>();
    
    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
    {
        return Results.BadRequest(new { errors = validationResults.Select(r => r.ErrorMessage) });
    }

    try
    {
        var result = await urlService.CreateShortUrlAsync(request.OriginalUrl);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error creating shortened URL: {ex.Message}");
    }
})
.WithName("CreateShortUrl")
.WithOpenApi()
.WithSummary("Create a shortened URL")
.WithDescription("Creates a shortened version of the provided URL");

// GET /{shortCode} - Redirect to original URL
app.MapGet("/{shortCode}", async (string shortCode, UrlShortenerService urlService) =>
{
    if (string.IsNullOrWhiteSpace(shortCode))
    {
        return Results.BadRequest("Short code is required");
    }

    try
    {
        var originalUrl = await urlService.GetOriginalUrlAsync(shortCode);
        
        if (originalUrl == null)
        {
            // Return 404 Not Found if short code doesn't exist
            return Results.NotFound("Short URL not found");
        }

        // Return HTTP 302 (temporary redirect) with Location header
        // Use 302 for temporary redirects (typical for URL shorteners)
        return Results.Redirect(originalUrl, permanent: false);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving URL: {ex.Message}");
    }
})
.WithName("RedirectToOriginalUrl")
.WithOpenApi()
.WithSummary("Redirect to original URL")
.WithDescription("Redirects to the original URL associated with the short code");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck")
.WithOpenApi();

app.Run(); 