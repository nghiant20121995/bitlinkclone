# API Usage Examples

This document provides examples of how to use the URL Shortener API endpoints.

## Prerequisites

1. Ensure MongoDB is running on your local machine or update the connection string in `appsettings.json`
2. Start the application using `dotnet run`
3. The API will be available at `https://localhost:7001`

## Testing the API

### 1. Create a Short URL

**Request:**
```bash
curl -X POST "https://localhost:7001/api/urls" \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://www.example.com/very-long-url-that-needs-shortening"
  }'
```

**Response:**
```json
{
  "originalUrl": "https://www.example.com/very-long-url-that-needs-shortening",
  "shortCode": "abc123",
  "shortenedUrl": "https://localhost:7001/abc123",
  "createdAt": "2025-01-03T10:00:00Z"
}
```

### 2. Test URL Redirection

**Request:**
```bash
curl -I "https://localhost:7001/abc123"
```

**Response:**
```
HTTP/2 302 
location: https://www.example.com/very-long-url-that-needs-shortening
```

### 3. Health Check

**Request:**
```bash
curl "https://localhost:7001/health"
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-01-03T10:00:00Z"
}
```

### 4. Testing with Different URLs

**Create multiple short URLs:**
```bash
# Example 1: GitHub
curl -X POST "https://localhost:7001/api/urls" \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://github.com/microsoft/vscode"}'

# Example 2: YouTube
curl -X POST "https://localhost:7001/api/urls" \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://www.youtube.com/watch?v=dQw4w9WgXcQ"}'

# Example 3: Wikipedia
curl -X POST "https://localhost:7001/api/urls" \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://en.wikipedia.org/wiki/URL_shortening"}'
```

## Using PowerShell (Windows)

If you're using PowerShell on Windows, you can use the following commands:

### Create Short URL
```powershell
$body = @{
    originalUrl = "https://www.example.com/very-long-url-that-needs-shortening"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7001/api/urls" -Method Post -Body $body -ContentType "application/json"
```

### Test Redirection
```powershell
Invoke-WebRequest -Uri "https://localhost:7001/abc123" -MaximumRedirection 0
```

## Browser Testing

1. **Access Swagger UI**: Open `https://localhost:7001/swagger` in your browser
2. **Test POST endpoint**: Use the Swagger interface to create short URLs
3. **Test redirection**: Copy the shortened URL and paste it in a new browser tab

## Error Scenarios

### Invalid URL
```bash
curl -X POST "https://localhost:7001/api/urls" \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "invalid-url"}'
```

**Response:**
```json
{
  "errors": ["The OriginalUrl field is not a valid fully-qualified http, https, or ftp URL."]
}
```

### Non-existent Short Code
```bash
curl -I "https://localhost:7001/nonexistent"
```

**Response:**
```
HTTP/2 404
```

## MongoDB Verification

You can verify the data in MongoDB using MongoDB Compass or the MongoDB shell:

```javascript
// Connect to MongoDB
use UrlShortener

// View all shortened URLs
db.urls.find().pretty()

// Find a specific short code
db.urls.findOne({shortCode: "abc123"})

// View click counts
db.urls.find({}, {shortCode: 1, clickCount: 1, originalUrl: 1}).sort({clickCount: -1})
```

## Performance Testing

For performance testing, you can use tools like Apache Bench (ab):

```bash
# Test POST endpoint (create a file with JSON payload first)
echo '{"originalUrl": "https://www.example.com/test-url"}' > payload.json

# Simple load test
ab -n 100 -c 10 -T application/json -p payload.json https://localhost:7001/api/urls
```

## Notes

- The API automatically handles duplicate URLs by returning the existing short code
- Click counts are incremented each time someone accesses a short URL
- MongoDB indexes are automatically created for optimal performance
- The short codes are 6 characters long and contain letters and numbers 