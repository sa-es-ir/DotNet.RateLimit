using DotNet.RateLimiter;
using DotNet.RateLimiter.Demo;
using DotNet.RateLimiter.Extensions;
using Microsoft.AspNetCore.Mvc;
// using StackExchange.Redis; // Uncomment for existing Redis connection example

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Standard usage - creates new Redis connections
builder.Services.AddRateLimitService(builder.Configuration);

// Example: Using existing Redis connection (uncomment to use)
/*
// Setup your existing Redis connection
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

// Use existing connection with rate limiting
builder.Services.AddRateLimitService(builder.Configuration, multiplexer);

// Or alternatively, use existing database
// var database = multiplexer.GetDatabase();
// builder.Services.AddRateLimitService(builder.Configuration, database);
*/


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/weatherforecast/{id}/{name}", (int id, string name, [FromQuery] string search, [FromQuery] int? age) =>
{
    return Results.Ok($"Hi I'm here! {id} - {name} - {search} - {age}");
})
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 2;
    options.QueryParams = "search,age";
    options.RouteParams = "id,name";
});

app.MapGet("/weatherforecast/{id}/{name}/another", (int id, string name, [FromQuery] string search, [FromQuery] int? age) =>
{
    return Results.Ok($"Hi I'm here! {id} - {name} - {search} - {age}");
})
.WithRateLimiter(options =>
{
    options.PeriodInSec = 60;
    options.Limit = 2;
    options.QueryParams = "search,age";
    options.RouteParams = "id,name";
});

app.Run();
