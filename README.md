# Hacker News Best Stories API

A RESTful ASP.NET Core Web API that retrieves the best n stories from the Hacker News API, sorted by score in descending order.

## Features

- Fetches best stories from Hacker News API
- Returns stories sorted by score (descending)
- Efficient caching to minimize external API calls
- Parallel fetching of story details for improved performance
- Input validation and error handling
- Configurable cache expiration and limits

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- An internet connection to access the Hacker News API

## How to Run

1. **Clone or download the repository**

2. **Navigate to the project directory**
   ```bash
   cd HackerNewsAPI
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

   The API will start on `https://localhost:5001` or `http://localhost:5000` (depending on your configuration).

6. **Access Swagger UI** (in Development mode)
   Navigate to `https://localhost:5001/swagger` to view the API documentation and test endpoints.

## API Endpoint

### GET /api/beststories

Retrieves the best n stories from Hacker News, sorted by score in descending order.

**Query Parameters:**
- `n` (optional, default: 10): Number of stories to retrieve (must be greater than 0, maximum: 500)

**Example Request:**
```bash
GET /api/beststories?n=10
```

**Example Response:**
```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  },
  {
    "title": "Another Story Title",
    "uri": "https://example.com/story",
    "postedBy": "username",
    "time": "2019-10-12T14:00:00+00:00",
    "score": 1500,
    "commentCount": 200
  }
]
```

**Response Codes:**
- `200 OK`: Successfully retrieved stories
- `400 Bad Request`: Invalid parameter (n <= 0 or n > 500)
- `500 Internal Server Error`: Error occurred while fetching stories

## Configuration

The application can be configured via `appsettings.json`:

```json
{
  "HackerNews": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0",
    "CacheExpirationMinutes": "5",
    "MaxStoriesLimit": "500"
  }
}
```

- **BaseUrl**: Base URL for the Hacker News API
- **CacheExpirationMinutes**: How long to cache story data (in minutes)
- **MaxStoriesLimit**: Maximum number of stories that can be requested in a single call

## Assumptions

1. **Caching Strategy**: 
   - The best stories list is cached for 5 minutes by default
   - Individual story details are cached for 5 minutes
   - This prevents overloading the Hacker News API while keeping data reasonably fresh

2. **Comment Count**:
   - Uses the `descendants` field from the Hacker News API if available
   - Falls back to counting `kids` array if `descendants` is null
   - Returns 0 if neither is available

3. **Time Format**:
   - Converts Unix timestamp to ISO 8601 format with UTC timezone offset (+00:00)

4. **Error Handling**:
   - Failed individual story fetches are logged but don't fail the entire request
   - Invalid stories (null or missing required fields) are filtered out

5. **Performance**:
   - Story details are fetched in parallel using `Task.WhenAll` for efficiency
   - Only fetches up to `MaxStoriesLimit` stories even if more IDs are available

6. **Input Validation**:
   - Maximum limit of 500 stories per request to prevent abuse
   - Minimum value of 1 story required

## Architecture

The application follows a clean architecture pattern:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic and external API communication
- **Models**: Data transfer objects and domain models
- **Caching**: In-memory caching using `IMemoryCache`

## Potential Enhancements

Given more time, the following enhancements could be implemented:

1. **Distributed Caching**: 
   - Replace in-memory cache with Redis or similar distributed cache for multi-instance deployments
   - Would improve scalability and cache sharing across instances

2. **Background Refresh Service**:
   - Implement a background service to periodically refresh the cache
   - Would ensure data is always fresh without waiting for cache expiration

3. **Rate Limiting**:
   - Add rate limiting middleware to prevent abuse
   - Could use libraries like `AspNetCoreRateLimit`

4. **Health Checks**:
   - Add health check endpoints to monitor API and Hacker News API availability
   - Useful for monitoring and alerting

5. **Response Compression**:
   - Enable response compression for large payloads
   - Would reduce bandwidth usage

6. **Pagination**:
   - Add pagination support for large result sets
   - Would improve performance for large n values

7. **Caching Strategy Improvements**:
   - Implement cache warming on application startup
   - Add cache hit/miss metrics for monitoring

8. **Retry Logic**:
   - Add exponential backoff retry logic for failed HTTP requests
   - Would improve resilience to transient failures

9. **Unit and Integration Tests**:
   - Add comprehensive test coverage
   - Mock external API calls for reliable testing

10. **API Versioning**:
    - Add API versioning support for future changes
    - Would allow backward compatibility

11. **Logging and Monitoring**:
    - Enhanced logging with structured logging (Serilog)
    - Integration with Application Insights or similar monitoring tools

12. **Docker Support**:
    - Add Dockerfile and docker-compose.yml for containerized deployment
    - Would simplify deployment and scaling

## License

This project is created as part of a coding assessment.

