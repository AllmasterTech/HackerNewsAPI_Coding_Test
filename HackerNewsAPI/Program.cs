using HackerNewsAPI.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Hacker News options
builder.Services.Configure<HackerNewsOptions>(options =>
{
    options.BaseUrl = builder.Configuration["HackerNews:BaseUrl"] ?? "https://hacker-news.firebaseio.com/v0";
    options.CacheExpirationMinutes = int.Parse(builder.Configuration["HackerNews:CacheExpirationMinutes"] ?? "5");
    options.MaxStoriesLimit = int.Parse(builder.Configuration["HackerNews:MaxStoriesLimit"] ?? "500");
});

// Add HTTP client for Hacker News API
builder.Services.AddHttpClient<IHackerNewsService, HackerNewsService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add memory cache
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size
});

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for easy API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hacker News Best Stories API v1");
    c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
});

// Skip HTTPS redirection to allow HTTP access
// app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

