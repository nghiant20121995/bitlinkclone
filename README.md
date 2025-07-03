# URL Shortener API (Like Bit.ly)

A simple, efficient URL shortener API built with .NET 8 Minimal API and MongoDB, similar to Bit.ly.

## Features

- **URL Shortening**: Convert long URLs into short, manageable links
- **URL Redirection**: Redirect users from short URLs to original URLs
- **Click Tracking**: Track how many times each short URL is accessed
- **Duplicate Prevention**: Reuse existing short codes for identical URLs
- **MongoDB Integration**: Efficient storage with optimized indexes
- **API Documentation**: Swagger/OpenAPI documentation available

## Technologies Used

- **.NET 8** - Minimal API
- **MongoDB** - NoSQL database for URL storage
- **MongoDB Driver** - Official .NET driver for MongoDB
- **Swagger/OpenAPI** - API documentation

## Prerequisites

- .NET 8 SDK
- MongoDB (local installation or cloud instance)
- MongoDB connection string

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd bitlinkclone
```

### 2. Open in Visual Studio

You can now open the project in Visual Studio by:

**Option A: Using the Solution File**
- Double-click `UrlShortener.sln` to open in Visual Studio
- Or open Visual Studio → File → Open → Project/Solution → Select `UrlShortener.sln`

**Option B: Using Command Line**
```bash
start UrlShortener.sln
```

**Option C: Using Visual Studio Code**
```bash
code .
```

### 3. Configure MongoDB

Update the `appsettings.json` file with your MongoDB connection details:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "UrlShortener",
    "CollectionName": "urls"
  },
  "BaseUrl": "https://localhost:7001"
}
```

### 4. Install Dependencies

```bash
dotnet restore
```

### 5. Run the Application

**Option A: Using Command Line**
```bash
dotnet run
```

**Option B: Using Visual Studio**
- Open `UrlShortener.sln` in Visual Studio
- Press `F5` or click the "Start" button (▶️)
- The application will start with debugging enabled

**Option C: Using Visual Studio Code**
- Open the project folder in VS Code
- Press `F5` or use the integrated terminal: `dotnet run`

The API will be available at `https://localhost:7001` and `http://localhost:5000`.

### 6. Access API Documentation

Open your browser and navigate to:
- Swagger UI: `https://localhost:7001/swagger`

## API Endpoints

### Create Short URL
- **POST** `/api/urls`
- **Request Body:**
  ```json
  {
    "originalUrl": "https://example.com/very-long-url-that-needs-shortening"
  }
  ```
- **Response:**
  ```json
  {
    "originalUrl": "https://example.com/very-long-url-that-needs-shortening",
    "shortCode": "abc123",
    "shortenedUrl": "https://localhost:7001/abc123",
    "createdAt": "2025-01-03T10:00:00Z"
  }
  ```

### Redirect to Original URL
- **GET** `/{shortCode}`
- **Example:** `GET /abc123`
- **Response:** HTTP 302 redirect to the original URL

### Health Check
- **GET** `/health`
- **Response:** Server health status

## MongoDB Document Structure

The application stores URL data in MongoDB using the following document structure:

| Field Name | Description | Data Type | Example |
|------------|-------------|-----------|---------|
| `_id` | Unique identifier for each record (auto-generated) | ObjectId | `507f1f77bcf86cd799439011` |
| `originalUrl` | The full URL provided by the user | String | `https://example.com/very-long-url` |
| `shortCode` | The unique short path generated for the URL | String | `abc123` |
| `createdAt` | Timestamp of when the record was created | DateTime | `2025-01-03T10:00:00Z` |
| `clickCount` | Number of times the short URL has been accessed | Integer | `42` |

### Sample MongoDB Document

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "originalUrl": "https://example.com/very-long-url-that-needs-shortening",
  "shortCode": "abc123",
  "createdAt": "2025-01-03T10:00:00Z",
  "clickCount": 42
}
```

## Database Indexes

The application automatically creates the following indexes for optimal performance:

1. **Index on `shortCode`** - Enables fast lookups when redirecting users
2. **Index on `originalUrl`** - Enables fast duplicate detection when creating new short URLs

## Architecture Features

### Short Code Generation
- Generates 6-character alphanumeric codes (A-Z, a-z, 0-9)
- Ensures uniqueness by checking against existing codes
- Approximately 56 billion possible combinations

### Duplicate URL Handling
- Checks for existing URLs before creating new short codes
- Returns existing short code if URL already exists
- Prevents unnecessary database growth

### Click Tracking
- Automatically increments click count on each redirect
- Provides usage analytics for each short URL

### Error Handling
- Comprehensive error handling for invalid URLs
- Proper HTTP status codes for different scenarios
- Detailed error messages for debugging

## Performance Considerations

- **Database Indexes**: Optimized for fast lookups and duplicate checks
- **Connection Pooling**: MongoDB driver manages connection pools automatically
- **Async Operations**: All database operations are asynchronous
- **Minimal API**: Lightweight framework with minimal overhead

## Security Notes

- Input validation for URLs
- CORS configuration for cross-origin requests
- No authentication required (add as needed for production)

## Development Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Run in development mode with hot reload
dotnet watch run
```

## Production Deployment

For production deployment, consider:

1. **Environment Variables**: Use environment variables for configuration
2. **Authentication**: Add authentication/authorization as needed
3. **Rate Limiting**: Implement rate limiting to prevent abuse
4. **Monitoring**: Add logging and monitoring
5. **SSL/TLS**: Ensure HTTPS in production
6. **Database Security**: Secure your MongoDB instance

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License. 