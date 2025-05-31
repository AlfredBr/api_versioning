using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "API Versioning Demo",
            Version = "v1",
            Description = "A simple example ASP.NET Core Web API - Version 1"
        }
    );

    c.SwaggerDoc(
        "v2",
        new OpenApiInfo
        {
            Title = "API Versioning Demo",
            Version = "v2",
            Description = "A simple example ASP.NET Core Web API - Version 2 with enhanced features"
        }
    );

    // Add document filter to separate endpoints by version
    c.DocumentFilter<ApiVersionDocumentFilter>();
});

// Configure logging
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Versioning Demo v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "API Versioning Demo v2");

    // Show more of the model by default
    c.DefaultModelExpandDepth(2);
    // Close all of the major nodes
    c.DocExpansion(DocExpansion.List);
    // Show the example by default
    c.DefaultModelRendering(ModelRendering.Example);
    // Turn on Try it by default
    c.EnableTryItOutByDefault();
    // Performance Requirement - sorry. Highlighting kills javascript rendering on big json
    c.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
    // Show the request duration
    c.ConfigObject.AdditionalItems.Add("displayRequestDuration", true);
});

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching"
};

// Version 1 API - Original functionality

app.MapGet(
        "/api/v1/weatherforecast",
        (ILogger<Program> logger) =>
        {
            logger.LogInformation("Weather forecast v1 requested at {RequestTime}", DateTime.Now);
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecastV1")
    .WithTags("Weather v1")
    .WithOpenApi(operation =>
        new(operation)
        {
            Summary = "Get weather forecast (v1)",
            Description =
                "Retrieves a 5-day weather forecast with random temperature and weather conditions"
        }
    )
    .Produces<WeatherForecast[]>(StatusCodes.Status200OK);

// Version 2 API - Enhanced with configurable days and additional data

app.MapGet(
        "/api/v2/weatherforecast",
        (ILogger<Program> logger, int days = 5) =>
        {
            logger.LogInformation(
                "Weather forecast v2 requested for {Days} days at {RequestTime}",
                days,
                DateTime.Now
            );

            // V2 enhancement: configurable number of days (max 14)
            days = Math.Min(Math.Max(days, 1), 14);

            var forecast = Enumerable
                .Range(1, days)
                .Select(index => new WeatherForecastV2(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)],
                    Random.Shared.Next(0, 101), // V2 enhancement: humidity percentage
                    Random.Shared.Next(0, 50) // V2 enhancement: wind speed km/h
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecastV2")
    .WithTags("Weather v2")
    .WithOpenApi(operation =>
        new(operation)
        {
            Summary = "Get weather forecast (v2)",
            Description =
                "Retrieves weather forecast with configurable days (1-14) and additional data like humidity and wind speed"
        }
    )
    .Produces<WeatherForecastV2[]>(StatusCodes.Status200OK);

// Backward compatibility - TODO: redirect old endpoint to v1

app.MapGet(
        "/weatherforecast",
        (ILogger<Program> logger) =>
        {
            logger.LogInformation(
                "Legacy weather forecast endpoint accessed, redirecting to v1 at {RequestTime}",
                DateTime.Now
            );
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecastLegacy")
    .WithTags("Weather Legacy")
    .WithOpenApi(operation =>
        new(operation)
        {
            Summary = "Get weather forecast (legacy)",
            Description =
                "Legacy endpoint - use /api/v1/weatherforecast or /api/v2/weatherforecast instead"
        }
    )
    .Produces<WeatherForecast[]>(StatusCodes.Status200OK);

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record WeatherForecastV2(
    DateOnly Date,
    int TemperatureC,
    string? Summary,
    int Humidity,
    int WindSpeed
)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class ApiVersionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var apiVersion = context.DocumentName;
        var pathsToRemove = new List<string>();

        foreach (var path in swaggerDoc.Paths)
        {
            var shouldKeep = apiVersion switch
            {
                "v1" => path.Key.Contains("/api/v1/") || path.Key == "/weatherforecast",
                "v2" => path.Key.Contains("/api/v2/"),
                _ => false
            };

            if (!shouldKeep)
            {
                pathsToRemove.Add(path.Key);
            }
        }

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }
    }
}
